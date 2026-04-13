namespace MMS.DAL.Models.MMS;

public partial class Bid
{
    public int Id { get; set; }

    public int CommitteeId { get; set; }

    public string ReferenceNumber { get; set; } = null!;

    public string? ExternalMeetingNumber { get; set; }

    public string Subject { get; set; } = null!;

    public string? Description { get; set; }

    public string? TeamLeaderUserId { get; set; }

    public int StatusId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime DueDate { get; set; }

    public int? MeetingId { get; set; }

    public string? InitialMinutesPath { get; set; }

    public string? FinalMinutesPath { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual Committee Committee { get; set; } = null!;

    public virtual BidStatus Status { get; set; } = null!;

    public virtual User? TeamLeader { get; set; }

    public virtual Meeting? Meeting { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<BidStakeholder> Stakeholders { get; set; } = new List<BidStakeholder>();

    public virtual ICollection<BidStatusHistory> StatusHistory { get; set; } = new List<BidStatusHistory>();

    public virtual ICollection<CommitteeItem> Items { get; set; } = new List<CommitteeItem>();
}
