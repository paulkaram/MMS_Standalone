using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
	internal class GroupMenuPermissionRepository : Repository<GroupMenuPermission>, IGroupMenuPermissionRepository
	{
		MmsContext ContextAsMMSContext => (Context as MmsContext)!;

		public GroupMenuPermissionRepository(DbContext context) : base(context)
		{
		}

		public async Task<bool> HasPermissionForGroupsAsync(List<string> groupIds, int permissionId)
		{
			return await ContextAsMMSContext.GroupMenuPermissions
				.AnyAsync(g => groupIds.Contains(g.GroupId) && g.PermissionId == permissionId);
		}

		public async Task<List<int>> GetPermissionIdsForGroupsAsync(List<string> groupIds)
		{
			return await ContextAsMMSContext.GroupMenuPermissions
				.Where(g => groupIds.Contains(g.GroupId))
				.Select(g => g.PermissionId)
				.ToListAsync();
		}
	}
}
