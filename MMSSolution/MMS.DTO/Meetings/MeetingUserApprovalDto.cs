using System;

namespace MMS.DTO.Meetings
{
	public record MeetingUserApprovalDto(
		int Id,
		string UserId,
		string UserName,
		int StatusId,
		string StatusName,
		DateTime? ApproveDate,
		string? Comment,
		int? AttachmentId,
		int? Version
	);
}
