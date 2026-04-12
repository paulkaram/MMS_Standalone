namespace MMS.DTO.Dashboard
{
	public class DashboardStatsDto
	{
		public int TotalMeetings { get; set; }
		public int UpcomingMeetings { get; set; }
		public int CompletedMeetings { get; set; }
		public int PendingTasks { get; set; }
		public int CompletedTasks { get; set; }
		public int OverdueTask { get; set; }
		public int TotalRecommendations { get; set; }
		public int PendingRecommendations { get; set; }
	}
}
