using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMS.DTO.Meetings
{
	public class MeetingDto
	{
		public MeetingDto()
		{
			MeetingAttendees = new List<MeetingAttendeePostDto>();
			MeetingAgenda = new List<MeetingAgendaDto>();

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
		public bool IsOnlineMeeting { get; set; }
		public string? MeetingUrl { get; set; }

		public List<MeetingAttendeePostDto> MeetingAttendees { get; set; }
		public List<MeetingAgendaDto> MeetingAgenda { get; set; }
	}
}
