using MapsterMapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Intalio.Tools.Common.Encryptions;
using Microsoft.Extensions.Configuration;
using Intalio.Tools.Common.Extensions.StringExtensions;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.BLL.Constants;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DAL.Models.MMS;

namespace MMS.BLL.Managers
{
    public class LookupManager
    {
        private readonly ISettingsUnitOfWork _settingsUnitOfWork;
        private readonly IUserManagementUnitOfWork _userManagementUnitOfWork;
        private readonly IMMSUnitOfWork _mmsUnitOfWork;
        private readonly IMapper _mapper;
        private readonly int _totalCountForAutoComplete;

        public LookupManager(IConfiguration configuration, ISettingsUnitOfWork settingsUnitOfWork, IUserManagementUnitOfWork userManagementUnitOfWork, IMMSUnitOfWork mmsUnitOfWork, IMapper mapper)
        {
            _settingsUnitOfWork = settingsUnitOfWork;
            _userManagementUnitOfWork = userManagementUnitOfWork;
            _mmsUnitOfWork = mmsUnitOfWork;
            _mapper = mapper;
            _totalCountForAutoComplete = configuration.GetValue<int>(AppSettingsConstants.TotalCountForAutoComplete);
        }

        public async Task<List<Dictionary>> ListDictionaryAsync()
        {
            var dictionary = await _settingsUnitOfWork.Dictionary.ListAsync();
            return dictionary.ToList();
        }

        public async Task<List<ListItemDto>?> ListStructureTypesAsync()
        {
            var structureTypes = await _settingsUnitOfWork.StructureTypes.ListAsync();
            return structureTypes.Select(x => _mapper.Map<ListItemDto>(x)).ToList();
        }

        public async Task<List<ListItemDto>?> ListStructuresAsync(LanguageDbEnum language)
        {
            var structures = await _settingsUnitOfWork.Structures.ListAsync();
            return structures.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
        }

        public async Task<List<ListItemDto>?> ListInternalStructuresAsync(LanguageDbEnum language)
        {
            var structures = await _settingsUnitOfWork.Structures.ListAsync(x => x.ExternalStructure == false && (x.IsDeleted == false || x.IsDeleted == null && x.Active == true));
            return structures.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
        }

        public async Task<List<ListItemDto>?> ListExternalStructuresAsync(LanguageDbEnum language)
        {
            var structures = await _settingsUnitOfWork.Structures.ListAsync(x => x.ExternalStructure == true && (x.IsDeleted == false || x.IsDeleted == null && x.Active == true));
            return structures.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
        }

        public async Task<List<ListItemDto>?> ListRolesAsync(LanguageDbEnum language)
        {
            var roles = await _settingsUnitOfWork.Roles.ListAsync();
            return roles.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
        }

        public async Task<List<ListItemDto>?> ListLanguagesAsync()
        {
            var langs = await _settingsUnitOfWork.Languages.ListAsync();
            return langs.Select(x => _mapper.Map<ListItemDto>(x)).ToList();
        }

        public async Task<List<ListItemDto>?> ListDataSourcesAsync()
        {
            var connections = await _settingsUnitOfWork.DataSources.ListAsync();
            return connections.Select(x => _mapper.Map<ListItemDto>(x)).ToList();
        }

        public async Task<List<string>> ListDataTablesAsync(int connectionId)
        {
            List<string> TableNames = new List<string>();
            var dataSource = await _settingsUnitOfWork.DataSources.GetAsync(x => x.Id == connectionId);
            if (dataSource != null)
            {
                var connection = string.Format("server={0};uid={1};pwd={2}; Database={3}; TrustServerCertificate=True", dataSource.InstanceName, dataSource.Username, EncryptionService.Decrypt(dataSource.Password), dataSource.Dbname);
                if (!string.IsNullOrWhiteSpace(connection))
                {
                    using (SqlConnection sqlConnection = new SqlConnection(connection))
                    {
                        sqlConnection.Open();
                        var schema = sqlConnection.GetSchema("Tables");

                        foreach (DataRow row in schema.Rows)
                        {
                            TableNames.Add(row[2].ToString());
                        }
                    }
                }
            }
            return TableNames;
        }

        public async Task<List<string>?> ListDataFieldsAsync(int connectionId, string tableName)
        {
            List<string> FieldNames = new List<string>();
            var dataSource = await _settingsUnitOfWork.DataSources.GetAsync(x => x.Id == connectionId);
            if (dataSource != null)
            {
                var connection = string.Format("server={0};uid={1};pwd={2}; Database={3};TrustServerCertificate=True", dataSource.InstanceName, dataSource.Username, EncryptionService.Decrypt(dataSource.Password), dataSource.Dbname);
                if (!string.IsNullOrWhiteSpace(connection))
                {
                    using (SqlConnection sqlConnection = new SqlConnection(connection))
                    {
                        sqlConnection.Open();
                        DataTable columnsTable = sqlConnection.GetSchema("Columns", new[] { null, null, tableName, null });
                        foreach (DataRow row in columnsTable.Rows)
                        {
                            FieldNames.Add(row["COLUMN_NAME"].ToString());
                        }
                    }
                }
            }
            return FieldNames;
        }


        public async System.Threading.Tasks.Task<List<ListItemDto>?> ListDataTypes()
        {
            await System.Threading.Tasks.Task.CompletedTask;
            return new List<ListItemDto>();
        }

        public async System.Threading.Tasks.Task<List<ListItemDto>?> ListOwnerTypes()
        {
            await System.Threading.Tasks.Task.CompletedTask;
            return new List<ListItemDto>();
        }

        public async Task<List<ListItemDto>> ListRolesForAutoCompleteAsync(string search, LanguageDbEnum language)
        {
            var roles = await _settingsUnitOfWork.Roles.ListAsync(x => x.RoleNameEn.Contains(search) || x.RoleNameAr.Contains(search));
            var retVal = roles.Take(_totalCountForAutoComplete).ToList();
            return retVal.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
        }

        public async Task<List<ListItemDto>?> ListStampsAsync(LanguageDbEnum language)
        {
            var stamps = await _settingsUnitOfWork.Stamps.ListAsync();
            return stamps.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
        }

        public async Task<ListItemDto?> GetRoleById(int roleId, LanguageDbEnum language)
        {
            var role = await _settingsUnitOfWork.Roles.GetAsync(x => x.Id == roleId);
            if (role != null)
            {
                return _mapper.Map<ListItemDto>((role, language));
            }
            return null;
        }

        public async Task<ListItemDto?> GetUserById(string userId, LanguageDbEnum language)
        {
            var user = await _userManagementUnitOfWork.Users.GetAsync(x => x.Id == userId);
            if (user != null)
            {
                return _mapper.Map<ListItemDto>((user, language));
            }
            return null;
        }

        public async Task<ListItemDto?> GetDepartmentById(int departmentId, LanguageDbEnum language)
        {
            var department = await _settingsUnitOfWork.Structures.GetAsync(x => x.Id == departmentId);
            if (department != null)
            {
                return _mapper.Map<ListItemDto>((department, language));
            }
            return null;
        }

        public async Task<List<ListItemDto>?> ListRoleTypes()
        {
            var roleTypes = await _settingsUnitOfWork.RoleTypes.ListAsync();
            return roleTypes.Select(x => _mapper.Map<ListItemDto>(x)).ToList();
        }

        public async Task<List<ListItemDto>?> ListAccountTypesAsync()
        {
            var accountTypes = await _settingsUnitOfWork.AccountTypes.ListAsync();
            return accountTypes.Select(x => _mapper.Map<ListItemDto>(x)).ToList();
        }

        public async Task<List<ListItemDto>?> ListPermissionLevelsAsync()
        {
            var permissionLevels = await _settingsUnitOfWork.PermissionLevels.ListAsync();
            return permissionLevels.Select(x => _mapper.Map<ListItemDto>(x)).ToList();
        }

        public async Task<List<ListItemDto>?> ListCommitteeTypesAsync(LanguageDbEnum language)
        {
            var committeeTypes = await _settingsUnitOfWork.CommitteeTypes.ListAsync();
            return committeeTypes.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
        }

        public async Task<List<ListItemDto>?> ListCommitteesAsync(LanguageDbEnum language)
        {
            var committees = await _settingsUnitOfWork.Committees.ListAsync();
            return committees.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
        }

        public async Task<List<ListItemDto>?> ListCommitteeRolesAsync(LanguageDbEnum language)
        {
            var committeeRoles = await _settingsUnitOfWork.CommitteeRoles.ListAsync();
            return committeeRoles.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
        }

        public async Task<List<ListItemDto>?> ListVotingTypesAsync(LanguageDbEnum language)
        {
            var votingTypes = await _settingsUnitOfWork.VotingTypes.ListAsync();
            return votingTypes.OrderBy(x => x.DisplayOrder).Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
        }

        public async Task<ListItemDto?> GetMeetingAsync(int meetingId)
        {
            var meeting = await _mmsUnitOfWork.Meetings.GetAsync(x=> x.Id == meetingId);
            return _mapper.Map<ListItemDto>(meetingId);
        }

		public async Task<List<ListItemDto>?> ListMeetingStatusesAsync(LanguageDbEnum language)
		{
			var meetingStatuses = await _settingsUnitOfWork.MeetingStatuses.ListAsync();
			return meetingStatuses.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
		}
        
        public async Task<List<ListItemDto>?> ListMeetingtypesAsync(LanguageDbEnum language)
		{
			var meetingTypes = await _settingsUnitOfWork.MeetingTypes.ListAsync();
			return meetingTypes.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
		}

		public async Task<List<ListItemDto>?> ListMeetingRecommendationsStatusesAsync(LanguageDbEnum language)
		{
			var statuses= await _settingsUnitOfWork.MeetingAgendaRecommendationStatuses.ListAsync(x=>x.Id!=(int)MeetingAgendaRecommendationStatusDbEnum.Draft);
			return statuses.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
		}

		public async Task<List<ListItemDto>?> ListRecommendationPrioritiesAsync(LanguageDbEnum language)
		{
			var priorities = await _settingsUnitOfWork.Priorities.ListAsync();
			return priorities.OrderBy(x => x.Id).Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
		}

		public async Task<List<ListItemDto>?> ListBranchesAsync(LanguageDbEnum language)
		{
			var branches = await _settingsUnitOfWork.Branches.ListAsync();
			return branches.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
		}

		public async Task<List<ListItemDto>?> ListPrivacies(LanguageDbEnum language)
		{
			var privacies = await _mmsUnitOfWork.Privacies.ListAsync();
			return privacies.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
		}

		public async Task<List<ListItemDto>?> ListCouncilsSessions(LanguageDbEnum language)
		{
			var sessions = await _mmsUnitOfWork.CouncilSessions.ListAsync();
			return sessions.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
		}

        public async Task<List<ListItemDto>?> ListCommitteeClassificationsAsync(LanguageDbEnum language)
        {
            var sessions = await _mmsUnitOfWork.CommitteeClassifications.ListAsync(x=>x.Active);
            return sessions.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
        }

        public async Task<List<ListItemDto>?> ListCommitteeStylesAsync(LanguageDbEnum language)
        {
            var sessions = await _mmsUnitOfWork.CommitteeStyles.ListAsync();
            return sessions.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
        }

        public async Task<List<ListItemDto>?> ListCommitteeStatusesAsync(LanguageDbEnum language)
        {
            var sessions = await _mmsUnitOfWork.CommitteeStatuses.ListAsync();
            return sessions.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
        }

    }
}
