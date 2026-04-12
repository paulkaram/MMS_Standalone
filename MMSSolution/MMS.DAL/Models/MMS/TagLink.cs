namespace MMS.DAL.Models.MMS;

public partial class TagLink
{
    public int Id { get; set; }

    public int TagId { get; set; }

    public int EntityTypeId { get; set; }

    public int EntityId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Tag Tag { get; set; } = null!;
}
