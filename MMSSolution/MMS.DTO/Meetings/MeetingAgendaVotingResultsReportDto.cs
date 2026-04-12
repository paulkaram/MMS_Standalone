
namespace MMS.DTO.Meetings
{
	public class MeetingAgendaVotingReportDto
	{
		public int Id { get; set; }
		public int TotalVotes { get; set; } = 0;
		public string Title { get; set; }
		public Dictionary<string, int> Votes { get; set; }
	}
}
