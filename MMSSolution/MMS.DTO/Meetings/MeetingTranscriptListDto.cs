namespace MMS.DTO.Meetings;

public class MeetingTranscriptListDto
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public int? AgendaId { get; set; }
    public string TranscriptText { get; set; } = string.Empty;
    public string? SummaryText { get; set; }
    public string? Language { get; set; }
    public string? AudioFileName { get; set; }
    public int? DurationSeconds { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? AttendeeUserId { get; set; }
    public string? AttendeeName { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
