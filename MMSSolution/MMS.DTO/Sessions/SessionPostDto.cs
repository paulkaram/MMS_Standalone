namespace MMS.DTO.Sessions
{
    public class SessionPostDto
    {
        public string ExternalReferenceNumber { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string? Note { get; set; }
        public DateTime MeetingDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? Tags { get; set; }
        public int CommitteeId { get; set; }
        public List<SessionItemPostDto> SessionItems { get; set; } = new();
    }
}
