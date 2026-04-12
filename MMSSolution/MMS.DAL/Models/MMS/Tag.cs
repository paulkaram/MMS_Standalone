namespace MMS.DAL.Models.MMS;

public partial class Tag
{
    public int Id { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string Color { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<TagLink> TagLinks { get; set; } = new List<TagLink>();
}
