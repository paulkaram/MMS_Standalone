namespace MMS.DTO.Permissions
{
	public class PermissionAccessListItemDto
	{
		public int Id { get; set; }
		public int LevelId { get; set; }
		public string? Name { get; set; }
		public string? GroupName { get; set; }
		public bool HasAccess { get; set; }
		public bool? HasLevel { get; set; }
    }
}
