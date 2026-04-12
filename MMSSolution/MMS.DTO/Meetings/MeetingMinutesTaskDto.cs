
namespace MMS.DTO.Meetings
{
	public record MeetingMinutesTaskDto(int MeetingId, List<string>UsersIds,int DueDate);

}
