namespace MMS.DTO.Committees
{
	public record UpdateCommitteeUserDto(string UserId,int CommitteeRoleId,short PrivacyId,string? Note,bool Active);

}
