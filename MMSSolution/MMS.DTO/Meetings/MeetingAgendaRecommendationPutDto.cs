namespace MMS.DTO.Meetings
{
    public record MeetingAgendaRecommendationPutDto(
        int Id,
        string Text,
        string? Description,
        string Owner,
        DateTime? DueDate,
        int? PriorityId,
        string? OwnerStructureId
    );
}

