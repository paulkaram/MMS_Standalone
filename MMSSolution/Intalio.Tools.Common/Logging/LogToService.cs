using Newtonsoft.Json;
using System.Text;

namespace Intalio.Tools.Common.Logging
{
    /// <summary>
    /// Service for logging activity to external audit service.
    /// DCC Compliance (NCA DCC-1:2022 Section 2-4): Enhanced with IP address, device info, and session tracking.
    /// </summary>
    public class LogToService
    {
        // Reuse HttpClient instance for better performance
        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(5) // Set reasonable timeout
        };

        public static Task LogActivityAsync(string logServiceUrl, string username, string userId, int operationId, int? processInstanceId,
            int? commentId, int? letterId, int? recordId, string actionName, string controllerName, string description, string additionalInfo,
            string? ipAddress = null, string? userAgent = null, string? sessionId = null, string? deviceInfo = null)
        {
            // Fire-and-forget: Don't block the response waiting for logging to complete
            _ = Task.Run(async () =>
            {
                try
                {
                    if (string.IsNullOrEmpty(logServiceUrl)) return;

                    StringContent content = new(JsonConvert.SerializeObject(new
                    {
                        username,
                        userId,
                        operationId,
                        processInstanceId,
                        commentId,
                        letterId,
                        recordId,
                        actionName,
                        controllerName,
                        description,
                        additionalInfo,
                        // DCC 2-4: Security audit fields
                        ipAddress,
                        userAgent,
                        sessionId,
                        deviceInfo
                    }), Encoding.UTF8, "application/json");

                    await _httpClient.PostAsync(logServiceUrl, content);
                }
                catch (Exception ex)
                {
                    LogToEventViewer.LogException(ex);
                }
            });

            return Task.CompletedTask;
        }
    }
}
