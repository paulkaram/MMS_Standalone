namespace MMS.DTO.CommitteeItems
{
    public class CommitteeItemDto
    {
        public int Id { get; set; }
        public int CommitteeId { get; set; }
        public string ReferenceNumber { get; set; } = null!;
        public string? ExternalReferenceNumber { get; set; }
        public string Content { get; set; } = null!;
        public int ItemTypeId { get; set; }
        public string? ItemTypeName { get; set; }
        public string? Tags { get; set; }
        public string? InternalNote { get; set; }
        public int? RelatedItemId { get; set; }
        public string? RelatedItemReferenceNumber { get; set; }
        public bool IsPrivate { get; set; }
        public int Order { get; set; }
        public string CreatedBy { get; set; } = null!;
        public string? CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<ListItemDto> TagList { get; set; } = new();
    }
}
