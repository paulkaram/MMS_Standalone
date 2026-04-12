using MMS.API.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMS.BLL.Constants;
using MMS.BLL.Managers;
using MMS.DTO;
using MMS.DTO.AppSettings;
using MMS.DTO.Meetings;
using MMS.API.Common.Attributes;
using MMS.DAL.Enumerations;
using Intalio.Tools.Common.Enumerations;
using Intalio.Tools.Common.Teams;
using Microsoft.OpenApi.Extensions;

namespace MMS.API.Controllers
{
    /// <summary>
    /// System settings controller.
    /// DCC Compliance (NCA DCC-1:2022 Section 2-4): System configuration changes are audited.
    /// </summary>
    [AllowAnonymous]
	[Route("api/systemSettings")]
	public class SystemSettingsController : IntalioBaseController
	{
        private readonly SettingManager _settingsManager;

        public SystemSettingsController(SettingManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        [HttpGet("theme-colors")]
		public async Task<IActionResult> ListThemeColors()
        {
            try
            {
                var themeColors = await _settingsManager.ListThemeColorsAsync();

                return Ok(new ApiResponseDto<ThemeColorsDto>(themeColors));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet]
		[RequiredPermission(PermissionDbEnum.SystemSettings, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> List()
        {
            try
            {
                var settings = await _settingsManager.ListAsync();

                return Ok(new ApiResponseDto<List<SettingsListItemDto>>(settings));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


        [HttpPut("theme-colors")]
		[RequiredPermission(PermissionDbEnum.SystemSettings, PermissionLevelDbEnum.Write)]
		[LogUserActivity(AuditOperationConstants.SettingsChange, "Updated theme colors")]
		public async Task<IActionResult> UpdateThemeColors(ThemeColorsDto themeColors)
        {
            try
            {
                await _settingsManager.UpdateThemeColorsAsync(themeColors);

                return Ok(new ApiResponseDto<object>(true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

		[HttpPut("setting")]
		[RequiredPermission(PermissionDbEnum.SystemSettings, PermissionLevelDbEnum.Write)]
		[LogUserActivity(AuditOperationConstants.SettingsChange, "Updated system setting {Name}")]
		public async Task<IActionResult> UpdateSettings(SettingsListItemDto setting)
		{
			try
			{
				await _settingsManager.UpdateSettingsAsync(setting);

				return Ok(new ApiResponseDto<object>(true));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}


		#region voting types
		[HttpGet("voting-type")]
		[RequiredPermission(PermissionDbEnum.VotingTypesSettings, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListVotingTypes()
		{
			try
			{
				var votingTypes = await _settingsManager.ListVotingTypes();

				return Ok(new ApiResponseDto<List<VotingTypeListItemDto>>(votingTypes));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpPost("voting-type")]
		[RequiredPermission(PermissionDbEnum.VotingTypesSettings, PermissionLevelDbEnum.Write)]
		[LogUserActivity(AuditOperationConstants.SettingsChange, "Added voting type")]
		public async Task<IActionResult> AddVotingTypes(VotingTypeListItemDto votingType)
		{
			try
			{
				 bool added = await _settingsManager.AddVotingType(votingType);

				return Ok(new ApiResponseDto<bool>(added,Success: true));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpPut("voting-type")]
		[RequiredPermission(PermissionDbEnum.VotingTypesSettings, PermissionLevelDbEnum.Write)]
		[LogUserActivity(AuditOperationConstants.SettingsChange, "Updated voting type {Id}")]
		public async Task<IActionResult> UpdateVotingTypes(VotingTypeListItemDto votingType)
		{
			try
			{
				bool added = await _settingsManager.UpdateVotingType(votingType);

				return Ok(new ApiResponseDto<bool>(added, Success: true));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpDelete("{votingTypeId}/voting-type")]
		[RequiredPermission(PermissionDbEnum.VotingTypesSettings, PermissionLevelDbEnum.Write)]
		[LogUserActivity(AuditOperationConstants.Delete, "Deleted voting type {votingTypeId}")]
		public async Task<IActionResult> DeleteVotingTypes(int votingTypeId)
		{
			try
			{
				(bool deleted, string message) = await _settingsManager.DeleteVotingType(votingTypeId, Language);

				return Ok(new ApiResponseDto<bool>(deleted, Success: deleted,Message:message));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("{votingTypeId}/voting-type-option")]
		[RequiredPermission(PermissionDbEnum.VotingTypesSettings, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListVotingTypesOptions(int votingTypeId)
		{
			try
			{
				var votingTypeOptions = await _settingsManager.ListVotingTypesOptions(votingTypeId);

				return Ok(new ApiResponseDto<List<VotingOptionsListItemDto>>(votingTypeOptions));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpPost("voting-type-option")]
		[RequiredPermission(PermissionDbEnum.VotingTypesSettings, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> AddVotingTypeOption(VotingOptionsListItemDto votingOption)
		{
			try
			{
				bool added = await _settingsManager.AddVotingTypeOption(votingOption);

				return Ok(new ApiResponseDto<bool>(added, Success: true));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpPut("voting-type-option")]
		[RequiredPermission(PermissionDbEnum.VotingTypesSettings, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> UpdateVotingTypeOption(VotingOptionsListItemDto votingOption)
		{
			try
			{
				bool added = await _settingsManager.UpdateVotingTypeOption(votingOption);

				return Ok(new ApiResponseDto<bool>(added, Success: true));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpDelete("{votingTypeOptionId}/voting-type-option")]
		[RequiredPermission(PermissionDbEnum.VotingTypesSettings, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> DeleteVotingTypeOption(int votingTypeOptionId)
		{
			try
			{
				(bool deleted,string message)  = await _settingsManager.DeleteVotingTypeOption(votingTypeOptionId,Language);

				return Ok(new ApiResponseDto<bool>(deleted, Success: deleted,Message:message));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

        [HttpGet("system-templates")]
        [RequiredPermission(PermissionDbEnum.SystemIdentity, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> ListSystemTemplates(int votingTypeId)
        {
            try
            {
                var templates = await _settingsManager.ListSystemTemplates();

                return Ok(new ApiResponseDto<List<SettingsListItemSubDto>>(templates));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        [HttpPut("system-template/{AppSettingId}")]
        [RequiredPermission(PermissionDbEnum.SystemIdentity, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.SettingsChange, "Updated system template {AppSettingId}")]
        public async Task<IActionResult> UpdateSystemTemplate(int AppSettingId)
        {
            try
            {
                FileStatusEnum status = FileStatusEnum.Valid;
                IFormCollection formCollection = await Request.ReadFormAsync();
                if (formCollection.Files.Count > 0)
                {
                    (status, _) = base.AreValidAttachments(formCollection.Files);

                }

                if (status != FileStatusEnum.Valid)
                {
                    return Ok(new ApiResponseDto<string>(Success: false, Message: status.GetDisplayName()));
                }
               var updated= await _settingsManager.UpdateSystemTemplate(AppSettingId, formCollection.Files);

                return Ok(new ApiResponseDto<bool>(updated));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }
        [HttpPut("reload-settings")]
        [RequiredPermission(PermissionDbEnum.SystemIdentity, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.SettingsChange, "Reloaded system settings")]
        public IActionResult ReloadSystemSettings()
        {
            try
            {

                bool reloaded = _settingsManager.ReloadSystemSettings();

                return Ok(new ApiResponseDto<bool>(reloaded));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Tests connection to Microsoft Teams / Graph API
        /// </summary>
        [HttpPost("test-teams-connection")]
        [RequiredPermission(PermissionDbEnum.SystemSettings, PermissionLevelDbEnum.Write)]
        public async Task<IActionResult> TestTeamsConnection([FromBody] TeamsConnectionTestDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.TenantId) || string.IsNullOrEmpty(dto.ClientId) || string.IsNullOrEmpty(dto.ClientSecret))
                {
                    return Ok(new ApiResponseDto<object>(null, Success: false, Message: "All fields are required"));
                }

                // Try to get an access token from Azure AD
                using var httpClient = new HttpClient();
                var tokenEndpoint = $"https://login.microsoftonline.com/{dto.TenantId}/oauth2/v2.0/token";

                var tokenRequest = new Dictionary<string, string>
                {
                    ["client_id"] = dto.ClientId,
                    ["client_secret"] = dto.ClientSecret,
                    ["scope"] = "https://graph.microsoft.com/.default",
                    ["grant_type"] = "client_credentials"
                };

                var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(tokenRequest));

                if (response.IsSuccessStatusCode)
                {
                    return Ok(new ApiResponseDto<object>(null, Success: true, Message: "Successfully connected to Microsoft Graph API"));
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Ok(new ApiResponseDto<object>(null, Success: false, Message: $"Connection failed: {response.StatusCode}"));
                }
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponseDto<object>(null, Success: false, Message: $"Error: {ex.Message}"));
            }
        }
        #endregion
    }

    public class TeamsConnectionTestDto
    {
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
}
