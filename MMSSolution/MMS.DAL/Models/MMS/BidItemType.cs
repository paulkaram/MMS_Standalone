namespace MMS.DAL.Models.MMS;

public partial class BidItemType
{
    public int Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
