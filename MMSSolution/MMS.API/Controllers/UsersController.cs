using Intalio.Tools.Common.Enumerations;
using Intalio.Tools.Common.Extensions.StringExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Constants;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.Notifications;
using MMS.DTO.Permissions;
using MMS.DTO.Users;
using MMS.DTO.Users.Auth;
using System.Security.Claims;

namespace MMS.API.Controllers
{
    /// <summary>
    /// Users controller.
    /// DCC Compliance (NCA DCC-1:2022 Section 2-1): User management actions are audited.
    /// </summary>
	[Route("api/users")]
    public class UsersController : IntalioBaseController
    {
        private readonly UserManagementManager _userManagementManager;

        public UsersController(UserManagementManager userManagementManager)
        {
            _userManagementManager = userManagementManager;
        }

        [HttpPost("logout")]
        [LogUserActivity(AuditOperationConstants.Logout, "User logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var logout = await _userManagementManager.Logout(UserId);
                return Ok(new ApiResponseDto<bool>(logout));
            }

            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("{userId}")]
        [RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> GetUser(string userId)
        {
            try
            {
                var user = await _userManagementManager.GetUserAsync(userId);
                return Ok(new ApiResponseDto<UserAdminListItemDto>(user));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost("language")]
        public async Task<IActionResult> UpdateLanguage([FromBody] string language)
        {
            try
            {
                await _userManagementManager.UpdateDefaultLanguageAsync(UserId, language);

                return Ok(new ApiResponseDto<bool>(true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost("{userId}/sms")]
        [RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Write)]
        public async Task<IActionResult> EnableSms(string userId, [FromQuery] bool activated)
        {
            try
            {
                await _userManagementManager.EnableSmsAsync(userId, activated);

                return Ok(new ApiResponseDto<object>(true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost("{userId}/email")]
        [RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Write)]
        public async Task<IActionResult> EnablEmail(string userId, [FromQuery] bool activated)
        {
            try
            {
                await _userManagementManager.EnableEmailAsync(userId, activated);

                return Ok(new ApiResponseDto<object>(true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost("signatures")]
        [LogUserActivity(AuditOperationConstants.SignatureAction, "User added signature")]
        public async Task<IActionResult> AddSignature([FromForm] byte[]? signature, [FromForm] int pincode)
        {
            try
            {
                if (signature == null)
                {
                    IFormCollection formcollection = await Request.ReadFormAsync();
                    if (formcollection.Files.Count == 1)
                    {
                        var fileStatus = IsPng(formcollection.Files[0]);
                        if (fileStatus == FileStatusEnum.Valid)
                        {
                            using MemoryStream ms = new();
                            formcollection.Files[0].CopyTo(ms);
                            signature = ms.ToArray();
                        }
                    }
                }

                if (signature != null)
                {
                    await _userManagementManager.SetSignatureAsync(UserId, signature, pincode);
                }

                return Ok(new ApiResponseDto<object>());
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("check-signature/{signatureId}/{pincode}")]
        public async Task<IActionResult> CheckSignature(int signatureId, string pincode)
        {
            try
            {
                bool hasAccess = await _userManagementManager.checkSignatureAccess(UserId, signatureId);
                if (hasAccess)
                {
                    var res = await _userManagementManager.CheckSignaturePincode(signatureId, pincode);
                    return Ok(new ApiResponseDto<bool>(res.approved, Success: res.approved, Message: res.message));
                }
                return Unauthorized();

            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }
        [HttpPost("profile-picture")]
        public async Task<IActionResult> UpdateProfilePicture()
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
                await _userManagementManager.UpdateProfilePicture(UserId, formCollection.Files);

                return Ok(new ApiResponseDto<bool>(true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }
        [AllowAnonymous]
        [HttpGet("profile-image/{userId}")]
        public async Task<IActionResult> GetUserProfileImage(string userId)
        {
            try
            {
                (byte[]? bytes, string mimeType) res = await _userManagementManager.GetUserProfileImage(userId);
                if (res.bytes != null && res.mimeType != null)
                {
                    return File(res.bytes, res.mimeType);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("signatures")]
        public async Task<IActionResult> ListSignatures()
        {
            try
            {
                List<ListItemDto> signatures = await _userManagementManager.ListSignaturesAsync(UserId);
                return Ok(new ApiResponseDto<List<ListItemDto>>(signatures));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("{userId}/roles")]
        [RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> ListUserStructuresRoles(string userId)
        {
            try
            {
                List<UserStructureRoleLstItemDto>? signatures = await _userManagementManager.ListUserStructuresRolesAsync(userId, base.Language);
                return Ok(new ApiResponseDto<List<UserStructureRoleLstItemDto>>(signatures));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpDelete("signatures/{signatureId}")]
        [LogUserActivity(AuditOperationConstants.Delete, "User deleted signature {signatureId}")]
        public async Task<IActionResult> DeleteSignature(int signatureId)
        {
            try
            {
                bool hasAccess = await _userManagementManager.checkSignatureAccess(UserId, signatureId);
                if (hasAccess)
                {
                    await _userManagementManager.DeleteSignatureAsync(signatureId);
                    return Ok(new ApiResponseDto<object>(null, true, string.Empty));
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("signature")]
        public async Task<IActionResult> GetSignature()
        {
            try
            {
                string? signature = await _userManagementManager.GetSignature(UserId);
                if (!string.IsNullOrWhiteSpace(signature))
                {
                    signature = $"data:image/png;base64,{signature}";
                }

                return Ok(new ApiResponseDto<string?>(signature));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("admin")]
        [RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> ListAdminUsers(int page = 1, int pageSize = 10, [FromQuery] bool active = true)
        {
            try
            {
                var users = await _userManagementManager.ListUsersForAdminAsync(page, pageSize, active);
                return Ok(new ApiResponseDto<GenericPaginationListDto<UserAdminListItemDto>>(users));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("{userId}/permissions")]
        [RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> GetUserPermissions(string userId)
        {
            try
            {
                var userPermissions = await _userManagementManager.ListPermissionsAsync(userId);
                return Ok(new ApiResponseDto<List<PermissionListItemDto>?>(userPermissions));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost("permissions/{userId}/{permissionId}/{permissionTypeId}")]
        [RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.PermissionChange, "Changed permission {permissionId} for user {userId}")]
        public async Task<IActionResult> EditUserPermissions(string userId, int permissionId, int permissionTypeId, [FromQuery] bool enabled)
        {
            try
            {
                await _userManagementManager.EditUserPermissionsAsync(userId, permissionId, permissionTypeId, enabled);
                return Ok(new ApiResponseDto<object>(true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("permissions")]
        public async Task<IActionResult> ListCurrentUserPermissions()
        {
            try
            {
                var userPermissions = await _userManagementManager.ListUserMenuPermissionsAsync(UserId);
                return Ok(new ApiResponseDto<List<PermissionAccessListItemDto>?>(userPermissions));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("current/unclaimed-tasks")]
        public async Task<IActionResult> CountUnclaimedTasks()
        {
            try
            {
                var count = await _userManagementManager.CountUnclaimedTasksAsync(UserId);
                return Ok(new ApiResponseDto<int>(count));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListUsers(string? search = null, bool active = true, SearchTypeEnum mode = SearchTypeEnum.List)
        {
            try
            {
                List<ListItemDto> users = new();
                switch (mode)
                {
                    case SearchTypeEnum.List:
                        users = await _userManagementManager.ListUsers(Language);
                        break;
                    case SearchTypeEnum.Autocomplete:
                        users = await _userManagementManager.ListUsersForAutoComplete(search ?? string.Empty, Language, active);
                        break;
                    default:
                        break;
                }
                return Ok(new ApiResponseDto<List<ListItemDto>>(users));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost]
        [RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.Create, "Created new user {Username}")]
        public async Task<IActionResult> AddUser(UserDto userObj)
        {
            try
            {
                await _userManagementManager.AddUserAsync(userObj);

                return Ok(new ApiResponseDto<object>(true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPut]
        [RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.Update, "Updated user {Id}")]
        public async Task<IActionResult> UpdateUser(UserDto userObj)
        {
            try
            {
                await _userManagementManager.UpdateUserAsync(userObj);

                return Ok(new ApiResponseDto<object>(true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> ListUserNotifications()
        {
            try
            {
                var notifications = await _userManagementManager.ListNotificationsAsync(UserId);
                return Ok(new ApiResponseDto<List<NotificationListItemDto>>(notifications));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }


    }
}
