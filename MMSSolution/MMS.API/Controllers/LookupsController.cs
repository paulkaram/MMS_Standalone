using Intalio.Tools.Common.Enumerations;
using Microsoft.AspNetCore.Mvc;
using MMS.BLL.Managers;
using MMS.DTO;
using MMS.API.Common;

namespace MMS.API.Controllers
{
    [Route("api/lookups")]
    public class LookupsController : IntalioBaseController
    {

        public readonly LookupManager _lookupManager;

        public LookupsController(LookupManager lookupManager)
        {
            _lookupManager = lookupManager;
        }


        [HttpGet("structure-types")]
        public async Task<IActionResult> ListStructureTypes()
        {
            try
            {
                var list = await _lookupManager.ListStructureTypesAsync();
                return Ok(new ApiResponseDto<List<ListItemDto>>(list));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("structures")]
        public async Task<IActionResult> ListStructures()
        {
            try
            {
                var list = await _lookupManager.ListStructuresAsync(Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(list));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
		[HttpGet("branches")]
		public async Task<IActionResult> ListBranches()
		{
			try
			{
				var list = await _lookupManager.ListBranchesAsync(Language);
				return Ok(new ApiResponseDto<List<ListItemDto>>(list));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("external-structures")]
        public async Task<IActionResult> ListExternalStructures()
        {
            try
            {
                var list = await _lookupManager.ListExternalStructuresAsync(Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(list));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("internal-structures")]
        public async Task<IActionResult> ListInternalStructures()
        {
            try
            {
                var list = await _lookupManager.ListInternalStructuresAsync(Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(list));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("roles")]
        public async Task<IActionResult> ListRoles(string search, SearchTypeEnum mode = SearchTypeEnum.List)
        {
            try
            {
                List<ListItemDto>? roles = new();
                switch (mode)
                {
                    case SearchTypeEnum.List:
                        roles = await _lookupManager.ListRolesAsync(Language);
                        break;
                    case SearchTypeEnum.Autocomplete:
                        roles = await _lookupManager.ListRolesForAutoCompleteAsync(search, Language);
                        break;
                    default:
                        break;
                }
                return Ok(new ApiResponseDto<List<ListItemDto>>(roles));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("roles/{roleId}")]
        public async Task<IActionResult> GetRoleById(int roleId)
        {
            try
            {
                var role = await _lookupManager.GetRoleById(roleId, Language);
                return Ok(new ApiResponseDto<ListItemDto>(role));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            try
            {
                var user = await _lookupManager.GetUserById(userId, Language);
                return Ok(new ApiResponseDto<ListItemDto>(user));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("departments/{departmentId}")]
        public async Task<IActionResult> GetDepartmentById(int departmentId)
        {
            try
            {
                var department = await _lookupManager.GetDepartmentById(departmentId, Language);
                return Ok(new ApiResponseDto<ListItemDto>(department));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("languages")]
        public async Task<IActionResult> ListLanguages()
        {
            try
            {
                var list = await _lookupManager.ListLanguagesAsync();
                return Ok(new ApiResponseDto<List<ListItemDto>>(list));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("data-sources")]
        public async Task<IActionResult> ListDataSources()
        {
            try
            {
                var list = await _lookupManager.ListDataSourcesAsync();
                return Ok(new ApiResponseDto<List<ListItemDto>>(list));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("data-tables")]
        public async Task<IActionResult> ListDataTables(int connectionId)
        {
            try
            {
                var list = await _lookupManager.ListDataTablesAsync(connectionId);
                return Ok(new ApiResponseDto<List<string>>(list));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("data-fields")]
        public async Task<IActionResult> ListDataFields(int connectionId, string tableName)
        {
            try
            {
                var list = await _lookupManager.ListDataFieldsAsync(connectionId, tableName);
                return Ok(new ApiResponseDto<List<string>>(list));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("data-types")]
        public async Task<IActionResult> ListDataTypes()
        {
            try
            {
                var list = await _lookupManager.ListDataTypes();
                return Ok(new ApiResponseDto<List<ListItemDto>>(list));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("role-types")]
        public async Task<IActionResult> ListRoleTypes()
        {
            try
            {
                var list = await _lookupManager.ListRoleTypes();
                return Ok(new ApiResponseDto<List<ListItemDto>>(list));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("owner-types")]
        public async Task<IActionResult> ListOwnerTypes()
        {
            try
            {
                var list = await _lookupManager.ListOwnerTypes();
                return Ok(new ApiResponseDto<List<ListItemDto>>(list));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("stamps")]
        public async Task<IActionResult> ListStamps()
        {
            try
            {
                var stamps = await _lookupManager.ListStampsAsync(Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(stamps));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("account-types")]
        public async Task<IActionResult> ListAccountTypes()
        {
            try
            {
                var accountTypes = await _lookupManager.ListAccountTypesAsync();
                return Ok(new ApiResponseDto<List<ListItemDto>>(accountTypes));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("permission-levels")]
        public async Task<IActionResult> ListPermissionLevels()
        {
            try
            {
                var permissionLevels = await _lookupManager.ListPermissionLevelsAsync();
                return Ok(new ApiResponseDto<List<ListItemDto>>(permissionLevels));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("committee-types")]
        public async Task<IActionResult> ListCommitteeTypes()
        {
            try
            {
                var committeeTypes = await _lookupManager.ListCommitteeTypesAsync(Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(committeeTypes));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("committees")]
        public async Task<IActionResult> ListCommittees()
        {
            try
            {
                var committees = await _lookupManager.ListCommitteesAsync(Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(committees));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("committee-roles")]
        public async Task<IActionResult> ListCommitteeRoles()
        {
            try
            {
                var committees = await _lookupManager.ListCommitteeRolesAsync(Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(committees));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("voting-types")]
        public async Task<IActionResult> ListVotingTypes()
        {
            try
            {
                var votingTypes = await _lookupManager.ListVotingTypesAsync(Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(votingTypes));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
		[HttpGet("meeting-statuses")]
		public async Task<IActionResult> ListMeetingStatuses()
		{
			try
			{
				var votingTypes = await _lookupManager.ListMeetingStatusesAsync(Language);
				return Ok(new ApiResponseDto<List<ListItemDto>>(votingTypes));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}
        
        [HttpGet("meeting-types")]
		public async Task<IActionResult> ListMeetingtypes()
		{
			try
			{
				var meetingTypes = await _lookupManager.ListMeetingtypesAsync(Language);
				return Ok(new ApiResponseDto<List<ListItemDto>>(meetingTypes));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("meeting-recommendations-statuses")]
		public async Task<IActionResult> ListMeetingRecommendationsStatuses()
		{
			try
			{
				var votingTypes = await _lookupManager.ListMeetingRecommendationsStatusesAsync(Language);
				return Ok(new ApiResponseDto<List<ListItemDto>>(votingTypes));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("recommendation-priorities")]
		public async Task<IActionResult> ListRecommendationPriorities()
		{
			try
			{
				var priorities = await _lookupManager.ListRecommendationPrioritiesAsync(Language);
				return Ok(new ApiResponseDto<List<ListItemDto>>(priorities));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("privacies")]
		public async Task<IActionResult> ListPrivacies()
		{
			try
			{
				var privacyList = await _lookupManager.ListPrivacies(Language);
				return Ok(new ApiResponseDto<List<ListItemDto>>(privacyList));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("meetings/{meetingId}")]
        public async Task<IActionResult> ListMeeting(int meetingId)
        {
            try
            {
                var meeting = await _lookupManager.GetMeetingAsync(meetingId);
                return Ok(new ApiResponseDto<ListItemDto>(meeting));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
		[HttpGet("council-sessions")]
		public async Task<IActionResult> ListCouncilsSessions()
		{
			try
			{
				var sessions = await _lookupManager.ListCouncilsSessions(Language);
				return Ok(new ApiResponseDto<List<ListItemDto>>(sessions));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}
        [HttpGet("committee-classification")]
        public async Task<IActionResult> ListCommitteeClassifications()
        {
            try
            {
                var committeeClassifications = await _lookupManager.ListCommitteeClassificationsAsync(Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(committeeClassifications));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        [HttpGet("committee-style")]
        public async Task<IActionResult> ListCommitteeStyles()
        {
            try
            {
                var committeeStyles = await _lookupManager.ListCommitteeStylesAsync(Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(committeeStyles));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("committee-status")]
        public async Task<IActionResult> ListCommitteeStatuses()
        {
            try
            {
                var committeeStatuses = await _lookupManager.ListCommitteeStatusesAsync(Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(committeeStatuses));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
    }
}
