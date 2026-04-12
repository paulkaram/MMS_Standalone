
using MMS.DAL.Core.Repositories.MMS;

namespace MMS.DAL.Core.UnitOfWork.MMS
{
    public interface ISettingsUnitOfWork : IUnitOfWork
    {
        IAppSettingRepository AppSettings { get; }
        IDictionaryRepository Dictionary { get; }
        IStructureTypeRepository StructureTypes { get; }
        IStructureRepository Structures { get; }
        IRoleRepository Roles { get; }
        IRoleTypeRepository RoleTypes { get; }
        ILanguageRepository Languages { get; }
        IEmailTemplateRepository EmailTemplates { get; }
        IDataSourceRepository DataSources { get; }
        IStampRepository Stamps { get; }
        IAccountTypeRepository AccountTypes { get; }
        IPermissionLevelRepository PermissionLevels { get; }
        ICommitteeTypeRepository CommitteeTypes { get; }
        ICommitteeRepository Committees { get; }
        ICommitteeRoleRepository CommitteeRoles { get; }
        IVotingTypeRepository VotingTypes { get; }
		IVotingTypeOptionRepository VotingTypeOptions { get; }
        IMeetingStatusRepository MeetingStatuses { get; }
        IMeetingTypeRepository MeetingTypes { get; }
		IMeetingAgendaRecommendationStatusRepository MeetingAgendaRecommendationStatuses { get; }
		IPriorityRepository Priorities { get; }
		IBranchesRepository Branches { get; }
		IMomTemplateRepository MomTemplates { get; }

	}
}
