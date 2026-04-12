using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Constants;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.Roles;

namespace MMS.API.Controllers
{
    /// <summary>
    /// System Roles controller (RBAC).
    /// Manages the [Role] table for system-wide role-based access control.
    /// </summary>
    [Route("api/roles")]
    [ApiController]
    public class RolesController : IntalioBaseController
    {
        private readonly MmsContext _context;

        public RolesController(MmsContext context)
        {
            _context = context;
        }

        [HttpGet]
        [RequiredPermission(PermissionDbEnum.Roles, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> ListRoles()
        {
            try
            {
                var roles = await _context.Roles
                    .OrderBy(r => r.RoleNameEn)
                    .Select(r => new
                    {
                        id = r.Id.ToString(),
                        nameAr = r.RoleNameAr,
                        nameEn = r.RoleNameEn,
                        typeId = r.TypeId.ToString(),
                        typeName = r.Type != null ? r.Type.Name : null,
                        isActive = true,
                        usersCount = r.UserRoles.Count + r.UserStructures.Count
                    })
                    .ToListAsync();

                return Ok(new ApiResponseDto<object>(roles));
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
                var role = await _context.Roles
                    .Where(r => r.Id == roleId)
                    .Select(r => new
                    {
                        id = r.Id.ToString(),
                        nameAr = r.RoleNameAr,
                        nameEn = r.RoleNameEn,
                        typeId = r.TypeId.ToString(),
                        isActive = true
                    })
                    .FirstOrDefaultAsync();

                if (role == null) return NotFound();
                return Ok(new ApiResponseDto<object>(role));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPost]
        [RequiredPermission(PermissionDbEnum.Roles, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.RoleAssignment, "Created new role")]
        public async Task<IActionResult> CreateRole([FromBody] CommitteeRoleDto roleDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleDto.NameEn) || string.IsNullOrWhiteSpace(roleDto.NameAr))
                    return BadRequest(new ApiResponseDto<object>(null, false, "Name in English and Arabic are required"));

                if (await _context.Roles.AnyAsync(r => r.RoleNameEn == roleDto.NameEn))
                    return Conflict(new ApiResponseDto<object>(null, false, "Role with this name already exists"));

                var role = new Role
                {
                    RoleNameAr = roleDto.NameAr,
                    RoleNameEn = roleDto.NameEn,
                    TypeId = 2 // Default to "User" type
                };

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponseDto<object>(new { id = role.Id }));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPut("{roleId}")]
        [RequiredPermission(PermissionDbEnum.Roles, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.RoleAssignment, "Updated role {roleId}")]
        public async Task<IActionResult> UpdateRole(int roleId, [FromBody] CommitteeRoleDto roleDto)
        {
            try
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
                if (role == null) return NotFound();

                role.RoleNameAr = roleDto.NameAr;
                role.RoleNameEn = roleDto.NameEn;

                await _context.SaveChangesAsync();
                return Ok(new ApiResponseDto<object>(true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpDelete("{roleId}")]
        [RequiredPermission(PermissionDbEnum.Roles, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.Delete, "Deleted role {roleId}")]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            try
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
                if (role == null) return NotFound();

                // Block delete if Admin (system role)
                if (role.RoleNameEn.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new ApiResponseDto<object>(null, false, "Cannot delete the Admin system role"));

                // Remove all user assignments first
                var userRoles = await _context.UserRoles.Where(ur => ur.RoleId == roleId).ToListAsync();
                _context.UserRoles.RemoveRange(userRoles);

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
                return Ok(new ApiResponseDto<object>(true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// List users assigned to a role.
        /// </summary>
        [HttpGet("{roleId}/users")]
        [RequiredPermission(PermissionDbEnum.Roles, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> GetRoleUsers(int roleId)
        {
            try
            {
                var users = await _context.UserRoles
                    .Where(ur => ur.RoleId == roleId)
                    .Select(ur => new
                    {
                        id = ur.User.Id,
                        username = ur.User.Username,
                        fullName = Language == LanguageDbEnum.Arabic ? ur.User.FullnameAr : ur.User.FullnameEn,
                        email = ur.User.Email
                    })
                    .ToListAsync();

                return Ok(new ApiResponseDto<object>(users));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Assign a user to a role.
        /// </summary>
        [HttpPost("{roleId}/users/{userId}")]
        [RequiredPermission(PermissionDbEnum.Roles, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.RoleAssignment, "Assigned user {userId} to role {roleId}")]
        public async Task<IActionResult> AssignUserToRole(int roleId, string userId)
        {
            try
            {
                if (await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId))
                    return Ok(new ApiResponseDto<object>(true));

                _context.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
                await _context.SaveChangesAsync();
                return Ok(new ApiResponseDto<object>(true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Remove a user from a role.
        /// </summary>
        [HttpDelete("{roleId}/users/{userId}")]
        [RequiredPermission(PermissionDbEnum.Roles, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.RoleAssignment, "Removed user {userId} from role {roleId}")]
        public async Task<IActionResult> RemoveUserFromRole(int roleId, string userId)
        {
            try
            {
                var userRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
                if (userRole != null)
                {
                    _context.UserRoles.Remove(userRole);
                    await _context.SaveChangesAsync();
                }
                return Ok(new ApiResponseDto<object>(true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
    }
}
