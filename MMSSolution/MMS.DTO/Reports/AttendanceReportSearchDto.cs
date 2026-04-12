namespace MMS.DTO.Reports
{
    public record AttendanceReportSearchDto (string? MeetingReferenceNo, DateTime? FromDate, DateTime? ToDate, string? Title);

}
