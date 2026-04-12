namespace MMS.DTO.Meetings
{
	public class MeetingInvitationMailDto
	{
		public MeetingInvitationMailDto() { 
		}
		public List<string> AttendeesEmails { get; set; }
		public string Title { get; set; }
		public string Notes { get; set; }
		public string Location { get; set; }
		public string CreatedByName { get; set; }

		public DateTime Date { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
        public string ReferenceNumber { get; set; }
		public List<MeetingAgendaDto> MeetingAgenda { get; set; } = new List<MeetingAgendaDto>();
    }
}
