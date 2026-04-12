namespace MMS.DTO.CommitteeItems
{
    public class CommitteeItemPostDto
    {
        public int CommitteeId { get; set; }
        public string? ExternalReferenceNumber { get; set; }
        public string Content { get; set; } = null!;
        public int ItemTypeId { get; set; }
        public string? Tags { get; set; }
        public string? InternalNote { get; set; }
        public int? RelatedItemId { get; set; }
        public bool IsPrivate { get; set; }
        public int Order { get; set; }
        public List<int> TagIds { get; set; } = new();
    }
}
