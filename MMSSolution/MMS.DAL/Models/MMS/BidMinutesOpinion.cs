namespace MMS.DAL.Models.MMS;

public partial class BidMinutesOpinion
{
    public int Id { get; set; }
    public int BidId { get; set; }
    public string? StakeholderUserId { get; set; }
    public int? ExternalMemberId { get; set; }
    public int? Opinion { get; set; }             // MinutesOpinionDbEnum: 1=Suitable, 2=Unsuitable
    public string? Comment { get; set; }
    public int StatusId { get; set; }             // VisionStatusDbEnum reused: 1=Draft, 2=Submitted
    public DateTime? SubmittedDate { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public virtual Bid Bid { get; set; } = null!;
    public virtual User? StakeholderUser { get; set; }
    public virtual ExternalMember? ExternalMember { get; set; }
}
