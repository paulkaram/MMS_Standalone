using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
	internal class RoleMenuPermissionRepository : Repository<RoleMenuPermission>, IRoleMenuPermissionRepository
	{
		MmsContext ContextAsMMSContext => (Context as MmsContext)!;

		public RoleMenuPermissionRepository(DbContext context) : base(context)
		{
		}

		public async Task<bool> HasPermissionForRolesAsync(List<string> roleNames, int permissionId)
		{
			return await ContextAsMMSContext.RoleMenuPermissions
				.AnyAsync(r => roleNames.Contains(r.RoleName) && r.PermissionId == permissionId);
		}

		public async Task<List<int>> GetPermissionIdsForRolesAsync(List<string> roleNames)
		{
			return await ContextAsMMSContext.RoleMenuPermissions
				.Where(r => roleNames.Contains(r.RoleName))
				.Select(r => r.PermissionId)
				.ToListAsync();
		}
	}
}
