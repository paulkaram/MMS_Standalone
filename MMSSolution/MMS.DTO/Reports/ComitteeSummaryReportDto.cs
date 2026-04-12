
namespace MMS.DTO.Reports
{
	public record ComitteeSummaryReportDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool IsActive { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int SubCommitteesCount { get; set; }
		public int MembersCount { get; set; }
		public int RecommendationsCount { get; set; }
		public int MeetingsCount { get; set; }
		public double AttendanceRate { get; set; }
		//public List<StatusMeetingsCountDto> StatusesMeetingsCount { get; set; } = new List<StatusMeetingsCountDto>();
	}
}
