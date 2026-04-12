namespace MMS.DTO.Permissions
{
	public class PermissionListItemDto
	{
		public int Id { get; set; }

		public string? TypeName { get; set; }

		public List<SecondLevelPermissionDto>? Items { get; set; }
	}
}
