namespace MMS.DTO.Committees
{
	public class ComitteesGeneralInfoListItemDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Code { get; set; }
		public string Description { get; set; }
		public bool HasChilds { get; set; }
		public int? ParentId { get; set; }
		public int TypeId { get; set; }
		public int ChildernsCount { get; set; }
		public bool ShowDetails { get; set; }
		public List<ListItemDto> Parents { get; set; } = new List<ListItemDto>();

	}

}
