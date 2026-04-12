namespace MMS.DAL.Models.MMS;

public class MeetingTranscript
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public int? AgendaId { get; set; }
    public string TranscriptText { get; set; } = string.Empty;
    public string? SummaryText { get; set; }
    public string? Language { get; set; }
    public string? AudioFileName { get; set; }
    public string? AudioFilePath { get; set; }
    public int? DurationSeconds { get; set; }
    public string Status { get; set; } = TranscriptStatus.Pending;
    public string? ErrorMessage { get; set; }
    public string? AttendeeUserId { get; set; }
    public string? AttendeeName { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public virtual Meeting? Meeting { get; set; }
}

public static class TranscriptStatus
{
    public const string Pending = "Pending";
    public const string Transcribing = "Transcribing";
    public const string Completed = "Completed";
    public const string Summarized = "Summarized";
    public const string Failed = "Failed";
}
