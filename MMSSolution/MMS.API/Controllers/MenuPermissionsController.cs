using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMS.API.Common;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.Permissions;

namespace MMS.API.Controllers
{
    [Route("api/menu-permissions")]
    public class MenuPermissionsController : IntalioBaseController
    {
        private readonly MmsContext _context;

        public MenuPermissionsController(MmsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// List all permissions where TypeId = 1 (Menu).
        /// </summary>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailablePermissions()
        {
            try
            {
                var permissions = await _context.Permissions
                    .Where(p => p.TypeId == (int)PermissionTypeDbEnum.Menu)
                    .OrderBy(p => p.Order)
                    .ThenBy(p => p.GroupItemOrder)
                    .Select(p => new MenuPermissionItemDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        GroupName = p.GroupName,
                        IsAssigned = false
                    })
                    .ToListAsync();

                return Ok(new ApiResponseDto<List<MenuPermissionItemDto>>(permissions));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// List local groups from MMS database.
        /// </summary>
        [HttpGet("iam-groups")]
        public async Task<IActionResult> GetGroups()
        {
            try
            {
                var groups = await _context.Groups
                    .Where(g => g.Active)
                    .OrderBy(g => g.NameEn)
                    .Select(g => new IamGroupDto
                    {
                        Id = g.Id.ToString(),
                        Name = g.NameEn,
                        NameAr = g.NameAr,
                        IsActive = g.Active
                    })
                    .ToListAsync();

                return Ok(new ApiResponseDto<List<IamGroupDto>>(groups));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// List local roles from MMS database.
        /// </summary>
        [HttpGet("iam-roles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _context.Roles
                    .OrderBy(r => r.RoleNameEn)
                    .Select(r => new IamRoleDto
                    {
                        Id = r.Id.ToString(),
                        Name = r.RoleNameEn,
                        NameAr = r.RoleNameAr,
                        IsActive = true
                    })
                    .ToListAsync();

                return Ok(new ApiResponseDto<List<IamRoleDto>>(roles));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Get assigned permission IDs for a group.
        /// </summary>
        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetGroupPermissions(string groupId)
        {
            try
            {
                var permissionIds = await _context.GroupMenuPermissions
                    .Where(g => g.GroupId == groupId)
                    .Select(g => g.PermissionId)
                    .ToListAsync();

                return Ok(new ApiResponseDto<List<int>>(permissionIds));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Get assigned permission IDs for a role.
        /// </summary>
        [HttpGet("role/{roleName}")]
        public async Task<IActionResult> GetRolePermissions(string roleName)
        {
            try
            {
                var permissionIds = await _context.RoleMenuPermissions
                    .Where(r => r.RoleName == roleName)
                    .Select(r => r.PermissionId)
                    .ToListAsync();

                return Ok(new ApiResponseDto<List<int>>(permissionIds));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Save menu permissions for a group. Deletes existing and inserts new.
        /// </summary>
        [HttpPost("group/{groupId}")]
        public async Task<IActionResult> SaveGroupPermissions(string groupId, [FromBody] SaveMenuPermissionsRequest request)
        {
            try
            {
                var existing = await _context.GroupMenuPermissions
                    .Where(g => g.GroupId == groupId)
                    .ToListAsync();
                _context.GroupMenuPermissions.RemoveRange(existing);

                var newPermissions = request.PermissionIds.Select(pid => new GroupMenuPermission
                {
                    GroupId = groupId,
                    GroupName = request.DisplayName,
                    PermissionId = pid,
                    CreatedDate = DateTime.Now
                }).ToList();

                await _context.GroupMenuPermissions.AddRangeAsync(newPermissions);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponseDto<object>(true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Save menu permissions for a role. Deletes existing and inserts new.
        /// </summary>
        [HttpPost("role/{roleName}")]
        public async Task<IActionResult> SaveRolePermissions(string roleName, [FromBody] SaveMenuPermissionsRequest request)
        {
            try
            {
                var existing = await _context.RoleMenuPermissions
                    .Where(r => r.RoleName == roleName)
                    .ToListAsync();
                _context.RoleMenuPermissions.RemoveRange(existing);

                var newPermissions = request.PermissionIds.Select(pid => new RoleMenuPermission
                {
                    RoleName = roleName,
                    PermissionId = pid,
                    CreatedDate = DateTime.Now
                }).ToList();

                await _context.RoleMenuPermissions.AddRangeAsync(newPermissions);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponseDto<object>(true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
    }
}
