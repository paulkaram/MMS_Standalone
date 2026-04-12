using Intalio.Tools.Common.JwtToken;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using MMS.BLL.Common.Security;
using MMS.BLL.Constants;
using MMS.BLL.Managers;
using System.Security.Cryptography;
using System.Text;

namespace MMS.API
{
    /// <summary>
    /// Action filter for logging user activity.
    /// DCC Compliance (NCA DCC-1:2022 Section 2-4): Enhanced with IP address, device info, session tracking, and PII masking.
    /// </summary>
    public class LogUserActivity : TypeFilterAttribute
    {
        public LogUserActivity(int operation, string descriptionString) : base(typeof(LogUserActivityActionFilter))
        {
            Arguments = new object[] { operation, descriptionString };
        }

        private class LogUserActivityActionFilter : IAsyncActionFilter
        {
            private readonly LogIntalioActivityManager _processForLogsManager;
            private readonly IDataMaskingService _dataMaskingService;
            private readonly int _operation;
            private readonly string _descriptionString;
            private readonly string _logActivityUrl;

            public LogUserActivityActionFilter(
                LogIntalioActivityManager processForLogsManager,
                IDataMaskingService dataMaskingService,
                int operation,
                string descriptionString,
                IConfiguration configuration)
            {
                _operation = operation;
                _descriptionString = descriptionString ?? "";
                _processForLogsManager = processForLogsManager;
                _dataMaskingService = dataMaskingService;
                _logActivityUrl = configuration.GetValue<string>(AppSettingsConstants.ActivityLogUrl) ?? string.Empty;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                await next();

                // DCC 2-4: Extract security audit information from HttpContext
                var httpContext = context.HttpContext;
                var ipAddress = GetClientIpAddress(httpContext);
                var userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault();
                var sessionId = GetSessionId(httpContext);
                var deviceInfo = GenerateDeviceFingerprint(httpContext);

                // DCC 2-4: Mask sensitive PII in parameters before logging
                var maskedParameters = MaskParameters(context.ActionArguments);

                await _processForLogsManager.LogActivity(_logActivityUrl,
                    username: UserManagementManager.GetStringClaimValue(context.HttpContext.User, JwtTokenGenerator.CommonClaimNames.FullName),
                    userId: UserManagementManager.GetStringClaimValue(context.HttpContext.User, JwtTokenGenerator.CommonClaimNames.UserId),
                    operation: _operation,
                    actionName: (context.ActionDescriptor as ControllerActionDescriptor)?.ActionName ?? "",
                    controllerName: (context.ActionDescriptor as ControllerActionDescriptor)?.ControllerName ?? "",
                    descriptionString: _descriptionString,
                    parameters: maskedParameters,
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    sessionId: sessionId,
                    deviceInfo: deviceInfo);
            }

            /// <summary>
            /// DCC 2-4: Mask sensitive PII in action parameters
            /// </summary>
            private IDictionary<string, object> MaskParameters(IDictionary<string, object?> parameters)
            {
                var result = new Dictionary<string, object>();
                var maskedParams = _dataMaskingService.MaskAuditParameters(parameters);
                foreach (var kvp in maskedParams)
                {
                    if (kvp.Value != null)
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                }
                return result;
            }

            /// <summary>
            /// DCC 2-4: Get client IP address, considering proxy headers
            /// </summary>
            private static string? GetClientIpAddress(HttpContext context)
            {
                // Check for forwarded IP (when behind proxy/load balancer)
                var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    // Take the first IP in the chain (original client)
                    return forwardedFor.Split(',').FirstOrDefault()?.Trim();
                }

                var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIp))
                {
                    return realIp;
                }

                return context.Connection.RemoteIpAddress?.ToString();
            }

            /// <summary>
            /// DCC 2-4: Get or generate session ID from JWT token
            /// </summary>
            private static string? GetSessionId(HttpContext context)
            {
                // Try to get session ID from JWT claims
                var jtiClaim = context.User?.FindFirst("jti")?.Value;
                if (!string.IsNullOrEmpty(jtiClaim))
                {
                    return jtiClaim;
                }

                // Fallback to user ID + timestamp hash for session tracking
                var userId = UserManagementManager.GetClaimValue(context.User, JwtTokenGenerator.CommonClaimNames.UserId);
                if (userId != 0)
                {
                    return $"session_{userId}_{DateTime.Now:yyyyMMddHH}";
                }

                return null;
            }

            /// <summary>
            /// DCC 2-4: Generate device fingerprint from request headers
            /// </summary>
            private static string? GenerateDeviceFingerprint(HttpContext context)
            {
                var components = new StringBuilder();
                components.Append(context.Request.Headers["User-Agent"].FirstOrDefault() ?? "");
                components.Append(context.Request.Headers["Accept-Language"].FirstOrDefault() ?? "");
                components.Append(context.Request.Headers["Accept-Encoding"].FirstOrDefault() ?? "");

                if (components.Length == 0)
                    return null;

                // Generate a hash of the fingerprint components
                var bytes = Encoding.UTF8.GetBytes(components.ToString());
                var hash = SHA256.HashData(bytes);
                return Convert.ToBase64String(hash)[..16]; // Truncate for storage
            }
        }
    }
}