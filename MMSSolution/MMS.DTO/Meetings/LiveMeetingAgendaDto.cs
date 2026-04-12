

namespace MMS.DTO.Meetings
{
	public class LiveMeetingAgendaDto
	{
		public int? Id { get; set; }
		public int MeetingId { get; set; }
		public string? Title { get; set; }
		public int Duration { get; set; }
		public int RemainingSeconds { get; set; }
		public string? Voting { get; set; }
		public string? Duty { get; set; }
		public DateTime? ActualStartDate { get; set; }

		public DateTime? ActualEndDate { get; set; }

		public bool? Paused { get; set; }

		public int? PauseDuration { get; set; }
		public int? CommitteeDutyId { get; set; }
		public int VotingTypeId { get; set; }
		public DateTime? LastPausedDate { get; set; }
		public string? Summary { get; set; }
		public List<MeetingTopicPostDto> AgendaTopics { get; set; }
		public VotingTypeListItemDto VotingType { get; set; }
		public List<MeetingUserVoteDto> MeetingUserVotes { get; set; }
		public bool IsRunning { get; set; } = false;
	}
}
