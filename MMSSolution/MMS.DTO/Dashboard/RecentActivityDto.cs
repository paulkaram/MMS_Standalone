namespace MMS.DTO.Dashboard
{
	public class RecentActivityDto
	{
		public int Id { get; set; }
		public string Type { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string? Description { get; set; }
		public DateTime Timestamp { get; set; }
		public string? UserId { get; set; }
		public string? UserName { get; set; }
	}
}
