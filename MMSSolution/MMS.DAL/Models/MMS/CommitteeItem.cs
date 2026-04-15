namespace MMS.DAL.Models.MMS;

public partial class CommitteeItem
{
    public int Id { get; set; }

    public int CommitteeId { get; set; }

    public int? BidId { get; set; }

    public string ReferenceNumber { get; set; } = null!;

    public string? ExternalReferenceNumber { get; set; }

    public string Content { get; set; } = null!;

    public int? ItemTypeId { get; set; }

    /// <summary>
    /// Procurement-style classification (Technical / Financial / Administrative / Legal
    /// / General). Orthogonal to <see cref="ItemTypeId"/> which classifies agenda items.
    /// Used by bid clauses and §5.11 procurement items.
    /// </summary>
    public int? BidItemTypeId { get; set; }

    public string? Tags { get; set; }

    public string? InternalNote { get; set; }

    public int? RelatedItemId { get; set; }

    public bool IsPrivate { get; set; }

    public int Order { get; set; }

    public DateTime? DueDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual Committee Committee { get; set; } = null!;

    public virtual Bid? Bid { get; set; }

    public virtual CommitteeItemType? ItemType { get; set; }

    public virtual BidItemType? BidItemType { get; set; }

    public virtual CommitteeItem? RelatedItem { get; set; }

    public virtual ICollection<CommitteeItem> InverseRelatedItem { get; set; } = new List<CommitteeItem>();

    public virtual User CreatedByNavigation { get; set; } = null!;
}
