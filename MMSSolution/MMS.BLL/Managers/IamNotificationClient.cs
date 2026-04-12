using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MMS.BLL.Managers;

/// <summary>
/// HTTP client for sending template-based notifications to the IAM Notification Center.
/// Sends template key + placeholder data — IAM resolves the template and delivers the notification.
/// </summary>
public class IamNotificationClient
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<IamNotificationClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public IamNotificationClient(IConfiguration configuration, ILogger<IamNotificationClient> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string templateKey, string userId,
        Dictionary<string, string> data, string? priority = null,
        string? actionUrl = null, string? metadata = null)
    {
        await PostAsync("notifications/send", new
        {
            templateKey, userId, data, priority, actionUrl, metadata
        });
    }

    public async Task SendBulkAsync(string templateKey, List<string> userIds,
        Dictionary<string, string> data, string? priority = null,
        string? actionUrl = null, string? metadata = null)
    {
        await PostAsync("notifications/send-bulk", new
        {
            templateKey, userIds, data, priority, actionUrl, metadata
        });
    }

    private async Task PostAsync(string endpoint, object payload)
    {
        try
        {
            var baseUrl = (_configuration["IAM:BaseUrl"] ?? "http://localhost:5100/api").TrimEnd('/');
            var apiKey = _configuration["IAM:NotificationApiKey"] ?? "";

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

            var content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOptions),
                Encoding.UTF8, "application/json");

            await client.PostAsync($"{baseUrl}/{endpoint}", content);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send notification to IAM: {Endpoint}", endpoint);
        }
    }
}
