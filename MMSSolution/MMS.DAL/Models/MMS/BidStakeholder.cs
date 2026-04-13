namespace MMS.DAL.Models.MMS;

public partial class BidStakeholder
{
    public int Id { get; set; }

    public int BidId { get; set; }

    public string? UserId { get; set; }

    public int? ExternalMemberId { get; set; }

    public bool IsTeamLeader { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Bid Bid { get; set; } = null!;

    public virtual User? User { get; set; }

    public virtual ExternalMember? ExternalMember { get; set; }
}
