using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace MMS.API.Common.Filters
{
    /// <summary>
    /// Rate limiting middleware to protect against brute force and DDoS attacks.
    /// DCC Compliance (NCA DCC-1:2022): Implements API rate limiting for security.
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly RateLimitSettings _settings;

        public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, RateLimitSettings settings)
        {
            _next = next;
            _cache = cache;
            _settings = settings;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip rate limiting for health checks and static files
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (path.Contains("/health") || path.Contains("/swagger"))
            {
                await _next(context);
                return;
            }

            var clientKey = GetClientKey(context);
            var endpoint = context.Request.Path.Value ?? "";

            // Apply stricter limits for authentication endpoints
            var isAuthEndpoint = endpoint.Contains("/auth", StringComparison.OrdinalIgnoreCase) ||
                                 endpoint.Contains("/login", StringComparison.OrdinalIgnoreCase) ||
                                 endpoint.Contains("/token", StringComparison.OrdinalIgnoreCase);

            var maxRequests = isAuthEndpoint ? _settings.AuthEndpointMaxRequests : _settings.MaxRequestsPerWindow;
            var windowSeconds = isAuthEndpoint ? _settings.AuthEndpointWindowSeconds : _settings.WindowSeconds;

            if (!IsRequestAllowed(clientKey, endpoint, maxRequests, windowSeconds, out var retryAfter))
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers.Append("Retry-After", retryAfter.ToString());
                context.Response.ContentType = "application/json";

                var response = new
                {
                    message = "Rate limit exceeded. Please try again later.",
                    retryAfterSeconds = retryAfter
                };
                await context.Response.WriteAsJsonAsync(response);
                return;
            }

            await _next(context);
        }

        private string GetClientKey(HttpContext context)
        {
            // Get client IP address
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            var clientIp = !string.IsNullOrEmpty(forwardedFor)
                ? forwardedFor.Split(',').FirstOrDefault()?.Trim()
                : context.Connection.RemoteIpAddress?.ToString();

            // Include user ID if authenticated for per-user rate limiting
            var userId = context.User?.FindFirst("userId")?.Value ?? "anonymous";

            return $"ratelimit_{clientIp}_{userId}";
        }

        private bool IsRequestAllowed(string clientKey, string endpoint, int maxRequests, int windowSeconds, out int retryAfter)
        {
            retryAfter = 0;
            var cacheKey = $"{clientKey}_{endpoint}";
            var now = DateTime.Now;

            var requestLog = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(windowSeconds);
                return new RequestLog { Requests = new List<DateTime>() };
            });

            if (requestLog == null)
            {
                return true;
            }

            lock (requestLog)
            {
                // Remove expired requests
                var windowStart = now.AddSeconds(-windowSeconds);
                requestLog.Requests.RemoveAll(r => r < windowStart);

                if (requestLog.Requests.Count >= maxRequests)
                {
                    // Calculate retry-after based on oldest request in window
                    var oldestRequest = requestLog.Requests.Min();
                    retryAfter = (int)Math.Ceiling((oldestRequest.AddSeconds(windowSeconds) - now).TotalSeconds);
                    retryAfter = Math.Max(1, retryAfter);
                    return false;
                }

                requestLog.Requests.Add(now);
                return true;
            }
        }

        private class RequestLog
        {
            public List<DateTime> Requests { get; set; } = new();
        }
    }

    /// <summary>
    /// Rate limiting configuration settings.
    /// DCC Compliance: Configurable rate limits for different endpoint types.
    /// </summary>
    public class RateLimitSettings
    {
        /// <summary>
        /// Maximum requests allowed per window for general endpoints
        /// </summary>
        public int MaxRequestsPerWindow { get; set; } = 100;

        /// <summary>
        /// Time window in seconds for general rate limiting
        /// </summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>
        /// Maximum requests allowed per window for authentication endpoints (stricter)
        /// </summary>
        public int AuthEndpointMaxRequests { get; set; } = 5;

        /// <summary>
        /// Time window in seconds for authentication endpoint rate limiting
        /// </summary>
        public int AuthEndpointWindowSeconds { get; set; } = 60;

        /// <summary>
        /// Whether to enable rate limiting
        /// </summary>
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// Extension methods for rate limiting middleware registration.
    /// </summary>
    public static class RateLimitingMiddlewareExtensions
    {
        public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection("RateLimiting").Get<RateLimitSettings>() ?? new RateLimitSettings();
            services.AddSingleton(settings);
            services.AddMemoryCache();
            return services;
        }

        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
        {
            var settings = app.ApplicationServices.GetService<RateLimitSettings>();
            if (settings?.Enabled == true)
            {
                app.UseMiddleware<RateLimitingMiddleware>();
            }
            return app;
        }
    }
}
