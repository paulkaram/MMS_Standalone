namespace MMS.DAL.Enumerations
{
	public enum PermissionDbEnum
	{
		ManageOrganization = 2,
		SystemSettings = 3,
		Dictionary = 4,
		Lookups = 5,
		Roles = 10,
		CouncilsAndCommittees = 13,
		CreateMeeting = 14,
		DraftMeetings = 15,
		Chat = 16,
		MMSTasks = 17,
		Meetings = 18,
		Recommendations = 20,
		CouncilsAndCommitteesGeneralInfo = 21,
		Dashboard = 22,
		ComitteesSummaryReport = 23,
		AttendanceReport = 24,
		VotingTypesSettings = 25,
		NotLinkedMeeting = 26,
		SystemPermissions = 28,
		SuperAdmin = 39,
        SystemIdentity = 40,
	}
	public enum CommitteePermissionDbEnum
	{
		CommitteeAddMeeting = 29,
		CommitteeRecommendation = 32,
		CommitteeMeetings = 33,
		CommitteeUsers = 34,
		CommitteeMeetingAttachments = 35,
		CommitteeMeetingMinutes = 37,
		VotingResults = 38,
        CommitteeActivities = 44,
		CommitteeAttachmentButtonAdd = 45,
        CommitteeAttachments = 46,
		CommitteeSessions = 47,
		CommitteeItems = 48
    }
	public enum PermissionLevelDbEnum
	{
		Full = 1,
		Read = 2,
		Write = 3,
	}

	public enum PermissionTypeDbEnum
	{
		Menu = 1,
		Committee = 2,
	}

    public enum CommitteeStatusDbEnum
    {
        Active = 1,
        Completed = 2,
        OnHold = 3
    }
}
