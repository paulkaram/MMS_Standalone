namespace MMS.DTO.Meetings
{
    public class MeetingPostDto
    {
        public MeetingPostDto()
        {
            MeetingAttendees = new List<MeetingAttendeePostDto>();
            MeetingAgendas = new List<MeetingAgendaListItemDto>();
            AssociatedMeetings = new List<AssociatedMeetingDto>();

		}
        public int? Id { get; set; }
        public string? Title { get; set; }
        public int? CommitteeId { get; set; }
        public int? CommitteeTypeId { get; set; }
        public DateTime? Date { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Location { get; set; }
        public string? CommitteeName { get; set; }
        public bool IsCommittee { get; set; }
        public bool ReadOnly { get; set; }
        public string? Notes { get; set; }
		public string? Createdby { get; set; }
		public int StatusId { get; set; }
        public int TypeId { get; set; }
        public string? Url { get; set; }
		public int? CouncilSessionId { get; set; }
		public string CouncilSessionName { get; set; }
		public bool IsOnlineMeeting { get; set; }


		public List<MeetingAttendeePostDto>? MeetingAttendees { get; set; }
        public List<MeetingAgendaListItemDto>? MeetingAgendas { get; set; }
        public List<AssociatedMeetingDto>? AssociatedMeetings { get; set; }
    }
}
