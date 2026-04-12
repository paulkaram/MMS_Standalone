namespace MMS.DTO.EmailTemplates
{
    public record EmailTemplateListItemDto(int Id, string AppCode, string Name, string? SendTo, string? Subject, string Body);

    public record UpdateEmailTemplateDto
    {
        public string? Subject { get; init; }
        public string Body { get; init; } = null!;
        public string? SendTo { get; init; }
    }
}
