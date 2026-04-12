namespace MMS.DTO.Users
{
	public record UserCommitteeListItemDto(string Id, int CommitteeRoleId, string Fullname, string Email, string RoleName, string PrivacyName,short PrivacyId,  bool Active, string? Note);
	
}
