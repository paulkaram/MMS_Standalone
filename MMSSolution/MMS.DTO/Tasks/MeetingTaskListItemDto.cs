namespace MMS.DTO.Tasks;

public record MeetingTaskListItemDto(
	int Id,
	int MeetingId,
	string MeetingReference,
	string MeetingTitle,
	bool IsDelayed,
	DateTime DueDate,
	int TypeId,
	string Type,
	bool? Claimed
);
