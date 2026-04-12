using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Security.Claims;
using Intalio.Tools.Common.JwtToken;
using MMS.BLL.Managers;

/// <summary>
/// Token validation middleware with concurrent session management.
/// </summary>
public class TokenValidationMiddleware
{
	private readonly RequestDelegate _next;
	private readonly IDatabase _redisDatabase;
	private readonly SessionLimitSettings _sessionSettings;

	public TokenValidationMiddleware(RequestDelegate next, IConnectionMultiplexer redis, SessionLimitSettings? sessionSettings = null)
	{
		_next = next;
		_redisDatabase = redis.GetDatabase();
		_sessionSettings = sessionSettings ?? new SessionLimitSettings();
	}

	public async Task InvokeAsync(HttpContext context)
	{
		if (context.User.Identity?.IsAuthenticated == true)
		{
			var userIdStr = ResolveUserId(context.User);

			if (!string.IsNullOrEmpty(userIdStr))
			{
				// Validate token against Redis
				var tokenKey = "token_" + userIdStr;
				var tokenInRedis = await _redisDatabase.StringGetAsync(tokenKey);
				var authorizationHeader = context.Request.Headers["Authorization"].ToString();

				if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
				{
					var tokenInHeader = authorizationHeader.Substring("Bearer ".Length).Trim();

					if (tokenInRedis.IsNullOrEmpty || tokenInHeader != tokenInRedis)
					{
						context.Response.StatusCode = StatusCodes.Status401Unauthorized;
						await context.Response.WriteAsync("Unauthorized: Token mismatch or not found");
						return;
					}

					// Session tracking
					if (_sessionSettings.EnableConcurrentSessionLimit)
					{
						var sessionValidation = await ValidateConcurrentSessions(userIdStr, tokenInHeader);
						if (!sessionValidation.IsValid)
						{
							context.Response.StatusCode = StatusCodes.Status401Unauthorized;
							context.Response.ContentType = "application/json";
							await context.Response.WriteAsJsonAsync(new
							{
								message = sessionValidation.Message,
								code = "SESSION_LIMIT_EXCEEDED"
							});
							return;
						}
					}

					await TrackSessionActivity(userIdStr, tokenInHeader, context);
				}
				else
				{
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					await context.Response.WriteAsync("Unauthorized: No token provided");
					return;
				}
			}
		}

		await _next(context);
	}

	private static string? ResolveUserId(ClaimsPrincipal user)
	{
		var mmsUserId = UserManagementManager.GetClaimValue(user, JwtTokenGenerator.CommonClaimNames.UserId);
		return mmsUserId != 0 ? mmsUserId.ToString() : null;
	}

	private async Task<(bool IsValid, string Message)> ValidateConcurrentSessions(string userId, string currentToken)
	{
		var sessionsKey = $"sessions_{userId}";

		try
		{
			var sessions = await _redisDatabase.HashGetAllAsync(sessionsKey);
			var activeSessions = sessions
				.Where(s => !s.Value.IsNullOrEmpty)
				.Select(s => new
				{
					Token = s.Name.ToString(),
					LastActivity = DateTime.TryParse(s.Value.ToString(), out var dt) ? dt : DateTime.MinValue
				})
				.Where(s => s.LastActivity > DateTime.Now.AddMinutes(-_sessionSettings.SessionTimeoutMinutes))
				.ToList();

			var isCurrentSessionTracked = activeSessions.Any(s => s.Token == currentToken);

			if (!isCurrentSessionTracked && activeSessions.Count >= _sessionSettings.MaxConcurrentSessions)
			{
				return (false, $"Maximum concurrent sessions ({_sessionSettings.MaxConcurrentSessions}) exceeded. Please logout from another device.");
			}

			return (true, string.Empty);
		}
		catch
		{
			return (true, string.Empty);
		}
	}

	private async Task TrackSessionActivity(string userId, string token, HttpContext context)
	{
		try
		{
			var sessionsKey = $"sessions_{userId}";
			var tokenHash = token.Length > 32 ? token[..32] : token;

			await _redisDatabase.HashSetAsync(sessionsKey, tokenHash, DateTime.Now.ToString("O"));
			await _redisDatabase.KeyExpireAsync(sessionsKey, TimeSpan.FromMinutes(_sessionSettings.SessionTimeoutMinutes * 2));

			var sessionMetaKey = $"session_meta_{userId}_{tokenHash}";
			var ipAddress = GetClientIpAddress(context);
			var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "";

			await _redisDatabase.HashSetAsync(sessionMetaKey, new HashEntry[]
			{
				new("ip", ipAddress ?? ""),
				new("userAgent", userAgent.Length > 200 ? userAgent[..200] : userAgent),
				new("lastActivity", DateTime.Now.ToString("O")),
				new("createdAt", DateTime.Now.ToString("O"))
			});
			await _redisDatabase.KeyExpireAsync(sessionMetaKey, TimeSpan.FromMinutes(_sessionSettings.SessionTimeoutMinutes));
		}
		catch
		{
			// Session tracking failure should not block the request
		}
	}

	private static string? GetClientIpAddress(HttpContext context)
	{
		var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
		if (!string.IsNullOrEmpty(forwardedFor))
		{
			return forwardedFor.Split(',').FirstOrDefault()?.Trim();
		}
		return context.Connection.RemoteIpAddress?.ToString();
	}
}

public class SessionLimitSettings
{
	public bool EnableConcurrentSessionLimit { get; set; } = true;
	public int MaxConcurrentSessions { get; set; } = 3;
	public int SessionTimeoutMinutes { get; set; } = 30;
}
