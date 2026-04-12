namespace MMS.DTO.Meetings
{
	public record SearchMeetingDto(int? StatusId,int? CommitteeId,int? MeetingId,DateTime? FromDate, DateTime? ToDate,string? Location,string? Title,bool IncludeDrafts=false,bool NoComitteeRelated=false);

}
