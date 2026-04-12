using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.Meetings;

namespace MMS.API.Common.Hubs
{
    public interface IMainHub
    {
        Task<List<string>> GetConnectedIdsAsync();
        Task NotifyAllClients();
        Task NotifyUser(string username);
        Task ClaimTask(string username);
        Task UserOnline();
        Task NotifyUsers(string[] usernames);
		Task NotifyChatUsers(string[] usernames);
		Task NotifyMeetingAgendaChange(string[] usernames,int meetingId, List<LiveMeetingAgendaDto> meetingAgenda);
		Task NotifyChatMessagesCount(string[] usernames);
		Task NotifyMeetingStatusChange(string[] usernames,int meetingId, MeetingStatusDbEnum newStatus);
		Task NotifyMeetingChatUsers(int meetingId, string[] usernames);
		Task ChangeMeetingAttendanceStatus(int meetingId, bool InMeeting);
		Task<string[]> GetMeetingOnlineAttendees(int meetingId);
		Task NotifyMeetingAttachmentsChange(string[] usernames,int meetingId, List<AttachmentListItemDto> meetingAttachments);
		Task NotifyNewMeetingAgendaBegins(string[] usernames, LiveMeetingAgendaDto meetingAgenda,string meetingTitle);
		Task NotifyMeetingAgendaNotesChange(int meetingId, int agendaId);
	}
}
