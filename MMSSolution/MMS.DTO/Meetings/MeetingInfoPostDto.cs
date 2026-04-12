namespace MMS.DTO.Meetings
{
    public record MeetingInfoPostDto(int Id, string Title, DateTime Date, string StartTime, string EndTime, string Location, bool IsCommittee, int? CommitteeId, int? CouncilSessionId, int TypeId, string? Url, string? Notes);
}
