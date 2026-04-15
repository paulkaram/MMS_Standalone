namespace MMS.DTO.Bids
{
    public class BidMinutesOpinionDto
    {
        public int Id { get; set; }
        public int BidId { get; set; }
        public string? StakeholderUserId { get; set; }
        public int? ExternalMemberId { get; set; }
        public string StakeholderName { get; set; } = null!;
        public bool IsExternal { get; set; }
        public int? Opinion { get; set; }
        public string? OpinionName { get; set; }
        public string? Comment { get; set; }
        public int StatusId { get; set; }
        public DateTime? SubmittedDate { get; set; }
    }

    public class BidMinutesOpinionPostDto
    {
        public int Opinion { get; set; }      // 1 = Suitable, 2 = Unsuitable
        public string? Comment { get; set; }
    }

    public class BidMinutesOpinionsSummaryDto
    {
        public int BidId { get; set; }
        public int TotalOpinions { get; set; }
        public int SubmittedOpinions { get; set; }
        public int SuitableCount { get; set; }
        public int UnsuitableCount { get; set; }
        public bool AllSubmitted { get; set; }
    }
}
