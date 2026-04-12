namespace MMS.DAL.Models.MMS;

public partial class CommitteeItemType
{
    public int Id { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public virtual ICollection<CommitteeItem> CommitteeItems { get; set; } = new List<CommitteeItem>();
}
