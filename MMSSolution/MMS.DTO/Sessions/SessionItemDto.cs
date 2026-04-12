namespace MMS.DTO.Sessions
{
    public class SessionItemDto
    {
        public int Id { get; set; }
        public string? ExternalId { get; set; }
        public string Subject { get; set; } = null!;
        public int ItemTypeId { get; set; }
        public string? ItemTypeName { get; set; }
        public string? Tags { get; set; }
        public string? InternalNote { get; set; }
        public int? RelatedSessionItemId { get; set; }
        public string? RelatedSessionItemSubject { get; set; }
        public int Order { get; set; }
    }
}
