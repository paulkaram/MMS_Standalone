namespace MMS.DTO.Bids
{
    public class BidPostDto
    {
        public int CommitteeId { get; set; }
        public string? ExternalMeetingNumber { get; set; }
        public string Subject { get; set; } = null!;
        public string? Description { get; set; }
        public string? TeamLeaderUserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public List<BidStakeholderPostDto> Stakeholders { get; set; } = new();
    }
}
