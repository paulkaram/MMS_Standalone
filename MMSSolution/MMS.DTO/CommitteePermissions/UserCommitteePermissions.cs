namespace MMS.DTO.CommitteePermissions
{
	public record UserCommitteePermissions(bool Users, bool Meetings, bool Recommendations, bool MeetingsAttachments, bool MeetingsVotingResults,bool MeetingsMinutes, bool CommitteeActivities, bool CommitteeAttachments, bool CommitteeAttachmentButtonAdd);
}
