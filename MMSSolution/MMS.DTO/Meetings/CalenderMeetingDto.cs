namespace MMS.DTO.Meetings
{

    public record CalenderMeetingDto(int Id, string? ReferenceNumber, int StatusId, int MeetingTypeId, string? Title, DateTime Date, string? StartTime, string? EndTime, string? MeetingUrl, bool? IsCommittee);

}
