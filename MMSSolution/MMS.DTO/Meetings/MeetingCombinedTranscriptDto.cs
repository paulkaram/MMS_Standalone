namespace MMS.DTO.Meetings;

public class MeetingCombinedTranscriptDto
{
    public int MeetingId { get; set; }
    public List<MeetingTranscriptListDto> AttendeeTranscripts { get; set; } = new();
    public string? CombinedSummary { get; set; }
    public int TotalAttendees { get; set; }
    public int CompletedTranscripts { get; set; }
}
