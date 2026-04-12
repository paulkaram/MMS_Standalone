using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.MMS
{
	public interface ICommitteePermissionRepository : IRepository<CommitteePermission>
	{
		Task<List<CommitteePermission>> ListIncludeCommitteeAsync(Expression<Func<CommitteePermission, bool>> filter);
	}
}
