namespace MMS.DTO.Bids
{
    public class BidItemVisionDto
    {
        public int Id { get; set; }
        public int BidId { get; set; }
        public int BidItemId { get; set; }
        public string? BidItemTitle { get; set; }
        public string? StakeholderUserId { get; set; }
        public int? ExternalMemberId { get; set; }
        public string StakeholderName { get; set; } = null!;
        public bool IsExternal { get; set; }
        public string? Comment { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } = null!;
        public DateTime? SubmittedDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
