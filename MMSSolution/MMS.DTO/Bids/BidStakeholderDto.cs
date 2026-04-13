namespace MMS.DTO.Bids
{
    public class BidStakeholderDto
    {
        public int Id { get; set; }
        public int BidId { get; set; }
        public string? UserId { get; set; }
        public int? ExternalMemberId { get; set; }
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public bool IsTeamLeader { get; set; }
        public bool IsExternal { get; set; }
    }
}
