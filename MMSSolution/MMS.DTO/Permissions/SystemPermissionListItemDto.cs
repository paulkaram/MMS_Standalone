
namespace MMS.DTO.Permissions
{
	public class SystemPermissionListItemDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string TypeName { get; set; }
		public string GroupName { get; set; }
		public string MapName { get; set; }
		public string Description { get; set; }
        public int Order { get; set; }
        public int? MapId { get; set; }
        public bool IsDefault { get; set; }

    }
}
