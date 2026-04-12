namespace MMS.DTO
{
	public record AttachmentListItemDto(int Id, string Name, string Type, int size, int RecordId, int RecordTypeId, int PrivacyId, string PrivacyName, string RecordTypeName, int Version, DateTime? CreatedDate);
}
