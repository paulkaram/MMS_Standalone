using MMS.DAL.Core.Repositories.MMS;

namespace MMS.DAL.Core.UnitOfWork.MMS
{
    public interface IMMSUnitOfWork : IUnitOfWork
	{
        ICommitteeRepository Committees { get; }
        IUserCommitteeRepository UserCommittee { get; }
        ICommitteeDutyRepository CommitteeDuty { get; }
        ICommitteeActivityRepository CommitteeActivity { get; }
        IMeetingRepository Meetings { get; }
        IMeetingNoteRepository MeetingNotes { get; }
        ITaskRepository Tasks { get; }
        IAttachmentRepository Attachments { get; }
        IMeetingAttendeeRepository MeetingAttendees { get; }
        IMeetingAgendaRepository MeetingAgendas { get; }
        IAgendaTopicRepository AgendaTopics { get; }
		IMeetingUserVoteRepository MeetingUserVotes { get; }
		IMeetingAgendaNoteRepository MeetingAgendaNotes { get; }
		IMeetingAgendaRecommendationsRepository MeetingAgendaRecommendations { get; }
		IRecommendationNoteRepository RecommendationNotes { get; }
        IVotingTypeOptionRepository VotingTypeOptions { get; }
		IPrivaciesRepository Privacies { get; }
		IPermissionRepository Permissions { get; }
		ICommitteePermissionRepository CommitteePermissions { get; }
		ICouncilSessionRepository CouncilSessions { get; }
		ICommitteeClassificationRepository CommitteeClassifications { get; }
		ICommitteeStylesRepository CommitteeStyles { get; }
		ICommitteeStatusesRepository CommitteeStatuses { get; }
		IPermissionMatrixRepository PermissionMatrices { get; }
		ISessionRepository Sessions { get; }
		ISessionItemRepository SessionItems { get; }
		ISessionItemTypeRepository SessionItemTypes { get; }
		ICommitteeItemRepository CommitteeItems { get; }
		ICommitteeItemTypeRepository CommitteeItemTypes { get; }
		ITagRepository Tags { get; }
		ITagLinkRepository TagLinks { get; }
		IExternalMemberRepository ExternalMembers { get; }
		ICommitteeExternalMemberRepository CommitteeExternalMembers { get; }
		IDelegationRepository Delegations { get; }
		IDelegationTaskRepository DelegationTasks { get; }
		IBidRepository Bids { get; }
		IBidStatusRepository BidStatuses { get; }
		IBidStakeholderRepository BidStakeholders { get; }
		IBidStatusHistoryRepository BidStatusHistory { get; }
		IBidItemVisionRepository BidItemVisions { get; }
		IBidMinutesOpinionRepository BidMinutesOpinions { get; }
		IBidItemTypeRepository BidItemTypes { get; }
		IProcurementProjectRepository ProcurementProjects { get; }
		ICompetitorRepository Competitors { get; }
		ICompetitorAttachmentRepository CompetitorAttachments { get; }
		IWorkflowTemplateRepository WorkflowTemplates { get; }
		IWorkflowStepRepository WorkflowSteps { get; }
		IWorkflowTransitionRepository WorkflowTransitions { get; }
		IWorkflowInstanceRepository WorkflowInstances { get; }
		IWorkflowTaskRepository WorkflowTasks { get; }
		IWorkflowHistoryRepository WorkflowHistory { get; }
		IRoleMenuPermissionRepository RoleMenuPermissions { get; }
		IGroupMenuPermissionRepository GroupMenuPermissions { get; }
		IUserGroupRepository UserGroups { get; }
		IUserRoleRepository UserRoles { get; }
		IRoleRepository Roles { get; }
		IGroupRepository Groups { get; }
		ICommitteeRoleRepository CommitteeRoles { get; }
		IViewerTokenRepository ViewerTokens { get; }

	}
}
