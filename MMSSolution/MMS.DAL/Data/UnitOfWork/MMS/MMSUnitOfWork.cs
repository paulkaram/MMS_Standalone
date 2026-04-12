using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Data.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.UnitOfWork.MMS
{
	internal class MMSUnitOfWork : UnitOfWork, IMMSUnitOfWork
	{
		public ICommitteeRepository Committees { get; private set; }

		public IUserCommitteeRepository UserCommittee { get; private set; }

		public ICommitteeDutyRepository CommitteeDuty { get; private set; }
		public ICommitteeActivityRepository CommitteeActivity { get; private set; }

		public IMeetingRepository Meetings { get; private set; }

		public IMeetingAttendeeRepository MeetingAttendees { get; private set; }

		public IAttachmentRepository Attachments { get; private set; }

		public IMeetingAgendaRepository MeetingAgendas { get; private set; }

		public IAgendaTopicRepository AgendaTopics { get; private set; }

		public ITaskRepository Tasks { get; private set; }

		public IMeetingNoteRepository MeetingNotes { get; private set; }
		public IMeetingUserVoteRepository MeetingUserVotes { get; private set; }
		public IMeetingAgendaNoteRepository MeetingAgendaNotes { get; private set; }
		public IMeetingAgendaRecommendationsRepository MeetingAgendaRecommendations { get; private set; }
		public IRecommendationNoteRepository RecommendationNotes { get; private set; }

		public IVotingTypeOptionRepository VotingTypeOptions { get; private set; }
		public IPrivaciesRepository Privacies { get; private set; }
		public IPermissionRepository Permissions { get; private set; }
		public ICommitteePermissionRepository CommitteePermissions { get; private set; }
		public ICouncilSessionRepository CouncilSessions { get; private set; }
		public IPermissionMatrixRepository PermissionMatrices { get; private set; }
		public ICommitteeClassificationRepository CommitteeClassifications { get; private set; }
		public ICommitteeStylesRepository CommitteeStyles { get; private set; }
		public ICommitteeStatusesRepository CommitteeStatuses { get; private set; }
		public ISessionRepository Sessions { get; private set; }
		public ISessionItemRepository SessionItems { get; private set; }
		public ISessionItemTypeRepository SessionItemTypes { get; private set; }
		public ICommitteeItemRepository CommitteeItems { get; private set; }
		public ICommitteeItemTypeRepository CommitteeItemTypes { get; private set; }
		public IRoleMenuPermissionRepository RoleMenuPermissions { get; private set; }
		public IGroupMenuPermissionRepository GroupMenuPermissions { get; private set; }
		public IUserGroupRepository UserGroups { get; private set; }
		public IUserRoleRepository UserRoles { get; private set; }
		public IRoleRepository Roles { get; private set; }
		public IViewerTokenRepository ViewerTokens { get; private set; }


		public MMSUnitOfWork(MmsContext context) : base(context)
		{
			Committees = new CommitteeRepository(context);
			UserCommittee = new UserCommitteeRepository(context);
			CommitteeDuty = new CommitteeDutyRepository(context);
            CommitteeActivity = new CommitteeActivityRepository(context);
			Meetings = new MeetingRepository(context);
			MeetingAttendees = new MeetingAttendeeRepository(context);
			Attachments = new AttachmentRepository(context);
			MeetingAgendas = new MeetingAgendaRepository(context);
			AgendaTopics = new AgendaTopicRepository(context);
			Tasks = new TasksRepository(context);
			MeetingNotes = new MeetingNoteRepository(context);
			MeetingUserVotes = new MeetingUserVoteRepository(context);
			MeetingAgendaNotes = new MeetingAgendaNoteRepository(context);
			MeetingAgendaRecommendations = new MeetingAgendaRecommendationsRepository(context);
			RecommendationNotes = new RecommendationNoteRepository(context);
			VotingTypeOptions = new VotingTypeOptionRepository(context);
			Privacies = new PrivacyRepository(context);
			Permissions = new PermissionRepository(context);
			CommitteePermissions = new CommitteePermissionRepository(context);
			CouncilSessions = new CouncilSessionRepository(context);
			PermissionMatrices = new PermissionMatrixRepository(context);
            CommitteeClassifications=new CommitteeClassificationRepository(context);
            CommitteeStyles=new CommitteeStylesRepository(context);
            CommitteeStatuses = new CommitteeStatusesRepository(context);
			Sessions = new SessionRepository(context);
			SessionItems = new SessionItemRepository(context);
			SessionItemTypes = new SessionItemTypeRepository(context);
			CommitteeItems = new CommitteeItemRepository(context);
			CommitteeItemTypes = new CommitteeItemTypeRepository(context);
			RoleMenuPermissions = new RoleMenuPermissionRepository(context);
			GroupMenuPermissions = new GroupMenuPermissionRepository(context);
			UserGroups = new UserGroupRepository(context);
			UserRoles = new UserRoleRepository(context);
			Roles = new RoleRepository(context);
			ViewerTokens = new ViewerTokenRepository(context);

        }
	}
}
