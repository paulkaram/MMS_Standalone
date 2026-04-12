namespace MMS.DTO.Meetings
{
	public record MeetingAgendaRecommendationListItemDto(
		int Id,
		string CreatedBy,
		DateTime CreatedAt,
		DateTime? DueDate,
		int MeetingAgendaId,
		string Text,
		int StatusId,
		string CreatedByName,
		string Owner,
		string OwnerName,
		int Percentage,
		string Status,
		int? PriorityId,
		string? PriorityName,
		string? OwnerStructureId,
		string? OwnerStructureName,
		string? Description
	);
}

