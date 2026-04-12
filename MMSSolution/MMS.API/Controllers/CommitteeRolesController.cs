using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.CommitteeRoles;

namespace MMS.API.Controllers
{
    [Route("api/committeeRoles")]
    [ApiController]
    public class CommitteeRolesController : IntalioBaseController
    {
        private readonly CommitteeRoleManager _committeeRoleManager;

        public CommitteeRolesController(CommitteeRoleManager committeeRoleManager)
        {
			_committeeRoleManager = committeeRoleManager;
        }

        [HttpGet()]
		[RequiredPermission(PermissionDbEnum.Roles, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListRoles()
        {
            try
            {
                var roles = await _committeeRoleManager.ListCommitteeRoles();
                return Ok(new ApiResponseDto<List<CommitteeRoleListItemDto?>>(roles, Success: true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPost]
		[RequiredPermission(PermissionDbEnum.Roles, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> CreateRole(CommitteeRoleDto role)
        {
            try
            {
                await _committeeRoleManager.CreateRole(role);

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPut("{roleId}")]
		[RequiredPermission(PermissionDbEnum.Roles, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> UpdateRole(int roleId, CommitteeRoleDto role)
        {
            try
            {
                await _committeeRoleManager.UpdateRole(roleId, role);
                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("{roleId}")]
		[RequiredPermission(PermissionDbEnum.Roles, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetRoleById(int roleId)
        {
            try
            {
                var role = await _committeeRoleManager.GetRoleById(roleId, Language);
                return Ok(new ApiResponseDto<ListItemDto>(role, Success: true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }

        }

        [HttpDelete("{roleId}")]
		[RequiredPermission(PermissionDbEnum.Roles, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> RemoveRoleById(int roleId)
        {
            try
            {
                await _committeeRoleManager.RemoveByCommitteeRoleId(roleId);
                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
    }
}
