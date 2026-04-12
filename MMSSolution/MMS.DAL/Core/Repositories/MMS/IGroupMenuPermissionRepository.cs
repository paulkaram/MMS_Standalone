using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
	public interface IGroupMenuPermissionRepository : IRepository<GroupMenuPermission>
	{
		Task<bool> HasPermissionForGroupsAsync(List<string> groupIds, int permissionId);
		Task<List<int>> GetPermissionIdsForGroupsAsync(List<string> groupIds);
	}
}
