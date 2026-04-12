namespace MMS.DTO
{
	public class TreeviewListItemDto
	{
		public string? Id { get; set; }
		public string? Name { get; set; }
		public int TypeId { get; set; }
		public bool IsActive { get; set; }
		public List<TreeviewListItemDto>? Children { get; set; }
	}
}
