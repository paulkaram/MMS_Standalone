using System.Text.Json.Serialization;

namespace MMS.DTO.Meetings
{
    public class MeetingAttendeeListItemDto
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = null!;

        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("jobTitle")]
        public string? JobTitle { get; set; }

        [JsonPropertyName("needsApproval")]
        public bool NeedsApproval { get; set; }

        [JsonPropertyName("approved")]
        public bool Approved { get; set; }

        [JsonPropertyName("attended")]
        public bool Attended { get; set; }
    }
}
