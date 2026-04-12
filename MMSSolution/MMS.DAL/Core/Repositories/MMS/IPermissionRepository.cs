using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
	public interface IPermissionRepository : IRepository<Permission>
	{
		Task<List<Permission>> ListByUserId(string userId);
		Task<int?> GetPermissionLevelByUserIdAndPermission(string userId, int permissionId);
		Task<List<Permission>> ListIncludeTypeAsync(System.Linq.Expressions.Expression<Func<Permission, bool>> filter);
		Task<List<Permission>> ListIncludeTypeAsync();
	}
}
