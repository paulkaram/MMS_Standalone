namespace MMS.DTO.Users
{
	public record UserAdminListItemDto(string Id, string FullnameAr, string FullnameEn, string Username, string Email, string Mobile, string NationalId,int DefaultLanguageId, bool Approved, bool SmsEnabled, bool EmailNotificationEnabled);
}
