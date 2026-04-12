namespace MMS.DTO.CouncilCommittees
{
	public class UserAdminCommitteesDto
	{
		public string UserId { get; set; } = null!;
		public List<int> CommitteeIds { get; set; } = new();
	}
}
