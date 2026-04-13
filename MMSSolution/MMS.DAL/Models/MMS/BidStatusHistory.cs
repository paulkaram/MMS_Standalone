namespace MMS.DAL.Models.MMS;

public partial class BidStatusHistory
{
    public int Id { get; set; }

    public int BidId { get; set; }

    public int? FromStatusId { get; set; }

    public int ToStatusId { get; set; }

    public string ChangedBy { get; set; } = null!;

    public DateTime ChangedDate { get; set; }

    public string? Note { get; set; }

    public virtual Bid Bid { get; set; } = null!;

    public virtual BidStatus? FromStatus { get; set; }

    public virtual BidStatus ToStatus { get; set; } = null!;

    public virtual User ChangedByNavigation { get; set; } = null!;
}
