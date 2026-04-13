namespace MMS.DTO.Bids
{
    public class BidStakeholderPostDto
    {
        public string? UserId { get; set; }
        public int? ExternalMemberId { get; set; }
        public bool IsTeamLeader { get; set; }
    }
}
