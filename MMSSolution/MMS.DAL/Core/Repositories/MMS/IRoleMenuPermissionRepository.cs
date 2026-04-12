using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
	public interface IRoleMenuPermissionRepository : IRepository<RoleMenuPermission>
	{
		Task<bool> HasPermissionForRolesAsync(List<string> roleNames, int permissionId);
		Task<List<int>> GetPermissionIdsForRolesAsync(List<string> roleNames);
	}
}
