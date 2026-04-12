using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Intalio.Tools.Common.Teams
{
    /// <summary>
    /// Service for creating Microsoft Teams online meetings via Graph API
    /// </summary>
    public class TeamsService
    {
        private readonly TeamsIntegrationSettings _settings;
        private readonly HttpClient _httpClient;
        private string? _accessToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public TeamsService(TeamsIntegrationSettings settings)
        {
            _settings = settings;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Checks if Teams integration is enabled and configured
        /// </summary>
        public bool IsEnabled =>
            _settings.Enabled &&
            !string.IsNullOrEmpty(_settings.TenantId) &&
            !string.IsNullOrEmpty(_settings.ClientId) &&
            !string.IsNullOrEmpty(_settings.ClientSecret);

        /// <summary>
        /// Creates an online Teams meeting
        /// </summary>
        public async Task<OnlineMeetingResult?> CreateOnlineMeetingAsync(
            string subject,
            DateTime startTime,
            DateTime endTime,
            List<string> attendeeEmails)
        {
            if (!IsEnabled)
                return null;

            try
            {
                var token = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                    return null;

                var organizerEmail = _settings.OrganizerEmail;
                if (string.IsNullOrEmpty(organizerEmail))
                    return null;

                var meetingRequest = new
                {
                    subject = subject,
                    startDateTime = startTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    endDateTime = endTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    participants = new
                    {
                        attendees = attendeeEmails.Select(email => new
                        {
                            upn = email,
                            role = "attendee"
                        }).ToArray()
                    },
                    lobbyBypassSettings = new
                    {
                        scope = "organization",
                        isDialInBypassEnabled = true
                    },
                    allowedPresenters = "organizer"
                };

                var requestContent = new StringContent(
                    JsonSerializer.Serialize(meetingRequest),
                    Encoding.UTF8,
                    "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(
                    $"https://graph.microsoft.com/v1.0/users/{organizerEmail}/onlineMeetings",
                    requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var meetingResponse = JsonSerializer.Deserialize<GraphOnlineMeetingResponse>(responseContent);

                    return new OnlineMeetingResult
                    {
                        Success = true,
                        JoinUrl = meetingResponse?.JoinWebUrl ?? string.Empty,
                        MeetingId = meetingResponse?.Id ?? string.Empty,
                        VideoTeleconferenceId = meetingResponse?.VideoTeleconferenceId ?? string.Empty,
                        DialInUrl = meetingResponse?.AudioConferencing?.DialinUrl ?? string.Empty,
                        TollNumber = meetingResponse?.AudioConferencing?.TollNumber ?? string.Empty,
                        ConferenceId = meetingResponse?.AudioConferencing?.ConferenceId ?? string.Empty
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new OnlineMeetingResult
                    {
                        Success = false,
                        ErrorMessage = $"Failed to create Teams meeting: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new OnlineMeetingResult
                {
                    Success = false,
                    ErrorMessage = $"Exception creating Teams meeting: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Deletes/cancels a Teams meeting
        /// </summary>
        public async Task<bool> DeleteOnlineMeetingAsync(string meetingId)
        {
            if (!IsEnabled || string.IsNullOrEmpty(meetingId))
                return false;

            try
            {
                var token = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                    return false;

                var organizerEmail = _settings.OrganizerEmail;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync(
                    $"https://graph.microsoft.com/v1.0/users/{organizerEmail}/onlineMeetings/{meetingId}");

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets an access token from Azure AD using client credentials
        /// </summary>
        private async Task<string?> GetAccessTokenAsync()
        {
            // Return cached token if still valid
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.Now < _tokenExpiry.AddMinutes(-5))
                return _accessToken;

            try
            {
                var tokenEndpoint = $"https://login.microsoftonline.com/{_settings.TenantId}/oauth2/v2.0/token";

                var tokenRequest = new Dictionary<string, string>
                {
                    ["client_id"] = _settings.ClientId,
                    ["client_secret"] = _settings.ClientSecret,
                    ["scope"] = "https://graph.microsoft.com/.default",
                    ["grant_type"] = "client_credentials"
                };

                var response = await _httpClient.PostAsync(
                    tokenEndpoint,
                    new FormUrlEncodedContent(tokenRequest));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

                    _accessToken = tokenResponse?.AccessToken;
                    _tokenExpiry = DateTime.Now.AddSeconds(tokenResponse?.ExpiresIn ?? 3600);

                    return _accessToken;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Result from creating an online meeting
    /// </summary>
    public class OnlineMeetingResult
    {
        public bool Success { get; set; }
        public string JoinUrl { get; set; } = string.Empty;
        public string MeetingId { get; set; } = string.Empty;
        public string VideoTeleconferenceId { get; set; } = string.Empty;
        public string DialInUrl { get; set; } = string.Empty;
        public string TollNumber { get; set; } = string.Empty;
        public string ConferenceId { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    // Internal classes for JSON deserialization
    internal class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    internal class GraphOnlineMeetingResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("joinWebUrl")]
        public string? JoinWebUrl { get; set; }

        [JsonPropertyName("videoTeleconferenceId")]
        public string? VideoTeleconferenceId { get; set; }

        [JsonPropertyName("audioConferencing")]
        public AudioConferencingInfo? AudioConferencing { get; set; }
    }

    internal class AudioConferencingInfo
    {
        [JsonPropertyName("dialinUrl")]
        public string? DialinUrl { get; set; }

        [JsonPropertyName("tollNumber")]
        public string? TollNumber { get; set; }

        [JsonPropertyName("conferenceId")]
        public string? ConferenceId { get; set; }
    }
}
