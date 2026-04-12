namespace MMS.DTO.Meetings
{
    public record MeetingAgendaRecommendationPostDto(
        int MeetingAgendaId,
        string Text,
        string? Description,
        string Owner,
        DateTime? DueDate,
        int? PriorityId,
        string? OwnerStructureId
    );
}

