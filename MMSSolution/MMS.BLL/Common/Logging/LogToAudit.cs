using Intalio.Tools.Common.Logging;
using MMS.DTO;
using System.Text;

namespace MMS.BLL.Common.Logging
{
    /// <summary>
    /// Audit logging wrapper.
    /// DCC Compliance (NCA DCC-1:2022 Section 2-4): Enhanced with IP address, device info, and session tracking.
    /// </summary>
    public class LogToAudit
    {
        public static async Task LogIntalioActivityAsync(string logActivityUrl, string username, string userId, int operationId,
            int? processInstanceId, int? commentId, int? letterId, int? recordId, string actionName, string controllerName,
            string description, string additionalInfo, string? ipAddress = null, string? userAgent = null,
            string? sessionId = null, string? deviceInfo = null)
        {
            await LogToService.LogActivityAsync(logActivityUrl, username, userId, operationId, processInstanceId, commentId,
                letterId, recordId, actionName, controllerName, description, additionalInfo, ipAddress, userAgent, sessionId, deviceInfo);
        }

        public static string GenerateSearchCriteria(List<ListItemDto> items)
        {
            if (items.Count > 0)
            {
                StringBuilder builder = new();
                builder.Append("<a-search>");
                foreach (var item in items)
                {
                    builder.Append($"<a-item><a-name>{item.Id}</a-name><a-value>{item.Name}</a-value></a-item>");
                }
                builder.Append("</a-search>");
                return builder.ToString();
            }
            return "";
        }
    }
}
