using Intalio.Tools.Common.JwtToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;

namespace MMS.API.Common.Attributes
{
    public class RequiredPermission : TypeFilterAttribute
    {
		//TODO: (han) this class can be modified to check on multiple permission with and options of OR and AND

        public RequiredPermission(PermissionDbEnum permission, PermissionLevelDbEnum permissionLevel) : base(typeof(RequiredPermissionAuthorizationFilter))
		{
			Arguments = new object[] { permission, permissionLevel };
		}

        private class RequiredPermissionAuthorizationFilter : AuthorizeAttribute, IAsyncAuthorizationFilter
        {
			private readonly UserManagementManager _userManagementManager;
			private readonly CouncilCommitteeManager _councilCommitteeManager;
			private readonly PermissionDbEnum _permission;
			private readonly PermissionLevelDbEnum _permissionLevel;


			public RequiredPermissionAuthorizationFilter(UserManagementManager userManagementManager, CouncilCommitteeManager councilCommitteeManager, PermissionDbEnum permission, PermissionLevelDbEnum permissionLevel)
			{
				_userManagementManager = userManagementManager;
				_councilCommitteeManager = councilCommitteeManager;
				_permission = permission;
				_permissionLevel = permissionLevel;

            }

			public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
			{
				string userId = UserManagementManager.GetStringClaimValue(context.HttpContext.User, JwtTokenGenerator.CommonClaimNames.UserId);

				if(!await _userManagementManager.HasUserPermissionAsync(userId, _permission, _permissionLevel, null, null))
				{
					// For CouncilsAndCommittees permission, also allow committee admin users
					if (_permission == PermissionDbEnum.CouncilsAndCommittees)
					{
						var adminResult = await _councilCommitteeManager.GetUserAdminCommitteesAsync(userId);
						if (adminResult.CommitteeIds.Count > 0)
							return; // User is a committee admin, allow access
					}

					context.Result = new JsonResult(new ApiResponseDto<object>(Success: false, Message: "AccessDenied"))
					{
						StatusCode = StatusCodes.Status403Forbidden
					};
				}
			}
		}


	}
}
