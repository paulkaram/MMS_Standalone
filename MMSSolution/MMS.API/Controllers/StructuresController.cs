using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.Structures;
using MMS.DTO.Users;

namespace MMS.API.Controllers
{
    [Route("api/structures")]
    public class StructuresController : IntalioBaseController
    {
        private readonly UserManagementManager _userManagementManager;
        private readonly StuctureManager _stuctureManager;

        public StructuresController(
            UserManagementManager userManagementManager,
            StuctureManager stuctureManager)
        {
            _userManagementManager = userManagementManager;
            _stuctureManager = stuctureManager;
        }

        [HttpGet]
		[RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListStructures()
        {
            try
            {
                var structures = await _userManagementManager.ListStructuresAsync(base.Language);

                return Ok(new ApiResponseDto<List<ListItemDto>>(structures));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }


        [HttpGet("external")]
		[RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListExternalStructures()
        {
            try
            {
                var structures = await _userManagementManager.ListExternalStructuresAsync(base.Language);

                return Ok(new ApiResponseDto<List<ListItemDto>>(structures));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("{structureId}")]
		[RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetStructure(int structureId)
        {
            try
            {
                var structure = await _stuctureManager.GetStructureAsync(structureId);

                return Ok(new ApiResponseDto<StructureDto>(structure));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("organization")]
		[RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListOrganization(bool onlyActive = true)
        {
            try
            {
                var organization = await _stuctureManager.ListOrganizationStructuresAsync(Language, onlyActive);

                return Ok(new ApiResponseDto<List<TreeviewListItemDto>>(organization));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("{structureId}/users")]
		[RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListUsersInStructure(int structureId)
        {
            try
            {
                var users = await _stuctureManager.ListUsersInStructureAsync(structureId, Language);

                return Ok(new ApiResponseDto<List<UserListItemDto>>(users));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("{structureId}/roles")]
		[RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Read)]
		public IActionResult ListRolesInStructure(int structureId)
        {
            try
            {
                var roles = _stuctureManager.ListRolesInStructureAsync(structureId, Language);

                return Ok(new ApiResponseDto<List<ListItemDto>>(roles));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpDelete("{structureId}/user/{userId}")]
		[RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> RemoveUserFromStructure(int structureId, string userId)
        {
            try
            {
                await _stuctureManager.RemoveUserFromStructureAsync(structureId, userId);

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost("{structureId}/add-user/{userId}/role/{roleId}")]
		[RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> AddUserToStructure(int structureId, string userId, int roleId)
        {
            try
            {
                await _stuctureManager.AddUserToStructureAsync(structureId, userId, roleId);

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost]
		[RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> AddStructure(StructureDto structure)
        {
            try
            {
                await _stuctureManager.CreateStructureAsync(structure);

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPut("{structureId}")]
		[RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> UpdateStructure(int structureId, StructureDto structure)
        {
            try
            {
                await _stuctureManager.UpdateStructureAsync(structureId, structure, UserId);

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpDelete("{structureId}")]
        [RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Write)]
        public async Task<IActionResult> DeleteStructure(int structureId)
        {
            try
            {
                await _stuctureManager.DeleteStructureAsync(structureId);

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        /// <summary>
        /// List users in a local structure (department) — alias for compatibility with frontend.
        /// </summary>
        [HttpGet("iam-department/{structureId}/users")]
        public async Task<IActionResult> ListDepartmentUsers(int structureId)
        {
            try
            {
                var users = await _stuctureManager.ListUsersInStructureAsync(structureId, Language);
                return Ok(new ApiResponseDto<List<UserListItemDto>>(users));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// List local organization tree — alias for compatibility with frontend.
        /// </summary>
        [HttpGet("iam-organization")]
        public async Task<IActionResult> ListOrganizationTree()
        {
            try
            {
                var organization = await _stuctureManager.ListOrganizationStructuresAsync(Language, true);
                return Ok(new ApiResponseDto<List<TreeviewListItemDto>>(organization));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
    }
}
