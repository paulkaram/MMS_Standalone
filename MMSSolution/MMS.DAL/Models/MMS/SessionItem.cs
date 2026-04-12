namespace MMS.DAL.Models.MMS;

public partial class SessionItem
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public string? ExternalId { get; set; }

    public string Subject { get; set; } = null!;

    public int ItemTypeId { get; set; }

    public string? Tags { get; set; }

    public string? InternalNote { get; set; }

    public int? RelatedSessionItemId { get; set; }

    public int Order { get; set; }

    public virtual Session Session { get; set; } = null!;

    public virtual SessionItemType ItemType { get; set; } = null!;

    public virtual SessionItem? RelatedSessionItem { get; set; }

    public virtual ICollection<SessionItem> InverseRelatedSessionItem { get; set; } = new List<SessionItem>();
}
