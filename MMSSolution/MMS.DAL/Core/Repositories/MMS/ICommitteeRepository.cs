using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface ICommitteeRepository : IRepository<Committee>
    {
        Task<List<int>> GetIdsByClassifications(IEnumerable<int?> classifications);
        Task<Committee?> GetIncludeUserAsync(Expression<Func<Committee, bool>> filter);
		Task<List<Committee>> ListIncludeMembersAsync(int page, int pageSize, Expression<Func<Committee, bool>>? filter = null);

		// Performance optimized batch methods
		Task<Dictionary<int, int>> GetCountsByParentIdsAsync(List<int> parentIds);
	}
}
