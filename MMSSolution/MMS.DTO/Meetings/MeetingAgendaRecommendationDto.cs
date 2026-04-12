namespace MMS.DTO.Meetings
{
	public record MeetingAgendaRecommendationDto(int Id, int MeetingAgendaId, string Text,DateTime? DueDate,string Owner);
}
