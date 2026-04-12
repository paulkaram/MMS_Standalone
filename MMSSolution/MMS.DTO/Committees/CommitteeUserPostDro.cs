namespace MMS.DTO.Committees
{
	public record CommitteeUserPostDro(int CommitteeId, string UserId, int CommitteeRoleId, short PrivacyId,bool Active,string? Note);
}
