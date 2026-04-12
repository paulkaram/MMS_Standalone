using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Data.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.UnitOfWork.MMS
{
    internal class SettingUnitOfWork : UnitOfWork, ISettingsUnitOfWork
    {
        public IAppSettingRepository AppSettings { get; private set; }
        public IDictionaryRepository Dictionary { get; private set; }
        public IStructureTypeRepository StructureTypes { get; private set; }
        public IStructureRepository Structures { get; private set; }
        public IRoleRepository Roles { get; private set; }
        public ILanguageRepository Languages { get; private set; }
        public IEmailTemplateRepository EmailTemplates { get; private set; }
        public IDataSourceRepository DataSources { get; private set; }
        public IPermissionRepository Permissions { get; private set; }
        public IStampRepository Stamps { get; private set; }
        public IRoleTypeRepository RoleTypes { get; private set; }
        public IAccountTypeRepository AccountTypes { get; private set; }
		public IPermissionLevelRepository PermissionLevels { get; private set; }
        public ICommitteeTypeRepository CommitteeTypes { get; private set; }
        public ICommitteeRepository Committees { get; private set; }
        public ICommitteeRoleRepository CommitteeRoles { get; private set; }
        public IVotingTypeRepository VotingTypes { get; private set; }
        public IVotingTypeOptionRepository VotingTypeOptions { get; private set; }
        public IMeetingStatusRepository MeetingStatuses { get; private set; }
        public IMeetingAgendaRecommendationStatusRepository MeetingAgendaRecommendationStatuses { get; private set; }
        public IPriorityRepository Priorities { get; private set; }
		public IBranchesRepository Branches { get; private set; }
        public IMomTemplateRepository MomTemplates { get; private set; }

        public IMeetingTypeRepository MeetingTypes { get; private set; }

        public SettingUnitOfWork(MmsContext context) : base(context)
        {
            AppSettings = new AppSettingRepository(context);
            Dictionary = new DictionaryRepository(context);
            StructureTypes = new StructureTypeRepository(context);
            Structures = new StructureRepository(context);
            Roles = new RoleRepository(context);
            Languages = new LanguageRepository(context);
            EmailTemplates = new EmailTemplateRepository(context);
            DataSources = new DataSourceRepository(context);
            Permissions = new PermissionRepository(context);
            Stamps = new StampRepository(context);
            RoleTypes = new RoleTypeRepository(context);
            AccountTypes = new AccountTypeRepository(context);
			PermissionLevels = new PermissionLevelRepository(context);
            CommitteeTypes = new CommitteeTypeRepository(context);
            Committees = new CommitteeRepository(context);
            CommitteeRoles = new CommitteeRoleRepository(context);
            VotingTypes = new VotingTypeRepository(context);
			MeetingStatuses= new MeetingStatusRepository(context);
			MeetingAgendaRecommendationStatuses= new MeetingAgendaRecommendationStatusRepository(context);
			Priorities = new PriorityRepository(context);
			VotingTypeOptions = new VotingTypeOptionRepository(context);
            MeetingTypes = new MeetingTypeRepository(context);
			Branches = new BranchesRepository(context);
            MomTemplates = new MomTemplateRepository(context);

		}
    }
}
