

namespace MMS.DTO.Meetings
{
	public class LiveMeetingDto
	{
		public LiveMeetingDto()
		{
			MeetingAttendees = new List<MeetingAttendeePostDto>();
			MeetingAgendas = new List<LiveMeetingAgendaDto>();

		}
		public int? Id { get; set; }
		public string? Title { get; set; }
		public int? CommitteeId { get; set; }
		public DateTime? Date { get; set; }
		public string? StartTime { get; set; }
		public string? EndTime { get; set; }
		public string? ReferenceNumber { get; set; }
		public string? Location { get; set; }
		public string? CommitteeName { get; set; }
		public bool IsCommittee { get; set; }
		public string? Notes { get; set; }
		public string? Createdby { get; set; }
		public int StatusId { get; set; }
		public string? StatusName { get; set; }
		public int TypeId { get; set; }
		public string? Url { get; set; }
		public int? CouncilSessionId { get; set; }
		public string CouncilSessionName { get; set; }
		public List<MeetingAttendeePostDto>? MeetingAttendees { get; set; }
		public List<LiveMeetingAgendaDto>? MeetingAgendas { get; set; }

	}
}
