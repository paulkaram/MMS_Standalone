

namespace MMS.DTO.Meetings
{
	public record MeetingAgendaNoteListItemDto(int Id,string CreatedBy,DateTime CreatedAt,int MeetingAgendaId, string Text,bool IsPublic,string CreatedByName);

}
