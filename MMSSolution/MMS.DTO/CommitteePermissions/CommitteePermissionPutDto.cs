namespace MMS.DTO.CommitteePermissions
{
	public class CommitteePermissionPutDto
	{
		public string UserId { get; set; } = null!;
		public int PermissionId { get; set; }
		public bool Enabled { get; set; }
	}
}
