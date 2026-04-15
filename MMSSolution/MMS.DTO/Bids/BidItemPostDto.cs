namespace MMS.DTO.Bids
{
    /// <summary>
    /// Minimal DTO for adding/editing items inside a bid.
    /// Item type is intentionally absent — bid items don't share the agenda
    /// taxonomy. A proper BidItemType taxonomy will land with §5.11.
    /// </summary>
    public class BidItemPostDto
    {
        public string ReferenceNumber { get; set; } = null!;
        public string? ExternalReferenceNumber { get; set; }
        public string Content { get; set; } = null!;
        public string? InternalNote { get; set; }
        public int Order { get; set; }
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Procurement classification (§5.11): Technical / Financial / Administrative / Legal / General.
        /// </summary>
        public int? BidItemTypeId { get; set; }

        /// <summary>
        /// Agenda-style type (§5.7 line 223): نوع البند (يُقرأ / بناءً على طلب…).
        /// Links to the existing CommitteeItemType lookup.
        /// </summary>
        public int? ItemTypeId { get; set; }

        /// <summary>Link to another CommitteeItem (§5.7 line 224 — البنود المرتبطة).</summary>
        public int? RelatedItemId { get; set; }

        /// <summary>Tag IDs from the shared Tag table; stored as TagLink rows keyed on the item.</summary>
        public List<int> TagIds { get; set; } = new();
    }
}
