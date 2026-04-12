namespace MMS.DTO.Dashboard;

public class ModuleDashboardSummaryDto
{
    public DashboardStatsDto Stats { get; set; } = new();
    public List<UpcomingMeetingDto> UpcomingMeetings { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    public RecommendationSummaryDto Recommendations { get; set; } = new();
}

public class RecommendationSummaryDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Completed { get; set; }
}
