namespace MMS.DTO.Dashboard
{
	public class UpcomingMeetingDto
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public DateTime Date { get; set; }
		public string Time { get; set; } = string.Empty;
		public string? Location { get; set; }
		public string? CouncilName { get; set; }
		public string Status { get; set; } = string.Empty;
	}
}
