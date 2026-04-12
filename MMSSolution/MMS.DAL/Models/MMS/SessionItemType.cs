namespace MMS.DAL.Models.MMS;

public partial class SessionItemType
{
    public int Id { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public virtual ICollection<SessionItem> SessionItems { get; set; } = new List<SessionItem>();
}
