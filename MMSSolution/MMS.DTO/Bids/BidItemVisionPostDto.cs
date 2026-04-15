namespace MMS.DTO.Bids
{
    public class BidItemVisionPostDto
    {
        public string? Comment { get; set; }
    }

    public class BidVisionsSummaryDto
    {
        public int BidId { get; set; }
        public int TotalVisions { get; set; }
        public int SubmittedVisions { get; set; }
        public int StakeholdersCount { get; set; }
        public int ItemsCount { get; set; }
        public bool AllSubmitted { get; set; }
        public List<BidVisionStakeholderProgressDto> ByStakeholder { get; set; } = new();
    }

    public class BidVisionStakeholderProgressDto
    {
        public string? UserId { get; set; }
        public int? ExternalMemberId { get; set; }
        public string Name { get; set; } = null!;
        public bool IsExternal { get; set; }
        public int Total { get; set; }
        public int Submitted { get; set; }
        public bool Completed { get; set; }
    }
}
