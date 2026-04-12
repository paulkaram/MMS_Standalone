namespace MMS.DTO.Meetings
{
	public class MeetingInfoDto
	{
		public int Id { set; get; }
		public string ReferenceNumber { set; get; }
		public string? CommitteeName { set; get; }
		public string Createdby { set; get; } = null!;
		public int StatusId { set; get; }
		public string Title { set; get; }
		public string Date { set; get; }
		public string StartTime { set; get; }
		public string EndTime { set; get; }
		public string Location { set; get; }
		public bool IsCommittee { set; get; }
		public int CommitteeId { set; get; }
		public int? CouncilSessionId { set; get; }
		public string? Notes { set; get; }
		public bool ReadOnly { set; get; }
		public bool IsOnlineMeeting { set; get; }
		public string? MeetingUrl { set; get; }
		public string? OnlineMeetingId { set; get; }
		public string? OnlineMeetingPasscode { set; get; }
		public List<MeetingAttendeePostDto> MeetingAttendees { get; set; }=new List<MeetingAttendeePostDto>() ;

	}
}
