namespace MMS.DTO.Bids
{
    public class BidDto
    {
        public int Id { get; set; }
        public int CommitteeId { get; set; }
        public string? CommitteeName { get; set; }
        public string ReferenceNumber { get; set; } = null!;
        public string? ExternalMeetingNumber { get; set; }
        public string Subject { get; set; } = null!;
        public string? Description { get; set; }
        public string? TeamLeaderUserId { get; set; }
        public string? TeamLeaderName { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public int StatusStepOrder { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int? MeetingId { get; set; }
        public string? InitialMinutesPath { get; set; }
        public string? FinalMinutesPath { get; set; }
        public string CreatedBy { get; set; } = null!;
        public string? CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int StakeholdersCount { get; set; }
        public int ItemsCount { get; set; }
        public bool IsOverdue { get; set; }
    }
}
