namespace MMS.DAL.Models.MMS;

public partial class BidStatus
{
    public int Id { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public int StepOrder { get; set; }

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
}
