namespace MMS.DTO.Meetings
{
    public record MeetingListItemDto(
        int Id,
        int StatusId,
        string? ReferenceNumber,
        string? Title,
        DateTime Date,
        string? StartTime,
        string? EndTime,
        string? Location,
        bool IsCommittee,
        string? Notes,
        string? CommitteeName,
        string StatusName,
        string? CreatedById,
        string? CreatedByName,
        int ApprovedCount,
        int TotalApprovalCount);
}
