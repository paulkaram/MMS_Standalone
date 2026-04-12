namespace MMS.DTO.Meetings
{

    public record MeetingAgendaRecommendationFollowUpListItemDto(int Id, string CreatedBy,DateTime? DueDate, DateTime CreatedAt, int MeetingAgendaId, string Text, int StatusId, string CreatedByName, string Owner, string OwnerName, int Percentage, string Status, int MeetingId, string MeetingReferenceNo, bool CanEdit);

}
