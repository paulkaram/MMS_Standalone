namespace MMS.DTO.Sessions
{
    public class SessionItemPostDto
    {
        public string? ExternalId { get; set; }
        public string Subject { get; set; } = null!;
        public int ItemTypeId { get; set; }
        public string? Tags { get; set; }
        public string? InternalNote { get; set; }
        public int? RelatedSessionItemId { get; set; }
        public int Order { get; set; }
    }
}
