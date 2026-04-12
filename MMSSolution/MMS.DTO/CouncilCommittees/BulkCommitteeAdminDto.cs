namespace MMS.DTO.CouncilCommittees
{
	public class BulkCommitteeAdminDto
	{
		public string UserId { get; set; } = null!;
		public List<int> CommitteeIds { get; set; } = new();
	}
}
