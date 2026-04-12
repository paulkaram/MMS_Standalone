using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class CommitteeRepository : Repository<Committee>, ICommitteeRepository
    {
        MmsContext ContextAsMMSContext => (Context as MmsContext)!;
        public CommitteeRepository(DbContext context) : base(context)
        {
        }

        public async Task<Committee?> GetIncludeUserAsync(Expression<Func<Committee, bool>> filter)
        {
            return await ContextAsMMSContext.Committees.Include(x=> x.UserCommittees)
                 .ThenInclude(x => x.User).FirstOrDefaultAsync(filter);
        }

		public async Task<List<Committee>> ListIncludeMembersAsync(int page, int pageSize, Expression<Func<Committee, bool>>? filter = null)
		{
			var query = ContextAsMMSContext.Committees.AsNoTracking().Include(x => x.UserCommittees);

			if (filter != null)
			{
				query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Committee, ICollection<UserCommittee>>)query.Where(filter);
			}

			return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
		}
        public async Task<List<int>> GetIdsByClassifications(IEnumerable<int?> classifications)
        {
           return  await ContextAsMMSContext.Committees.Where(x => classifications.Contains(x.CommitteeClassificationId)).Select(x=>x.Id).ToListAsync();
        }

		// Performance optimized batch methods

		/// <summary>
		/// Get sub-committee counts grouped by parent IDs in a single query
		/// </summary>
		public async Task<Dictionary<int, int>> GetCountsByParentIdsAsync(List<int> parentIds)
		{
			return await ContextAsMMSContext.Committees.AsNoTracking()
				.Where(x => x.ParentId.HasValue && parentIds.Contains(x.ParentId.Value))
				.GroupBy(x => x.ParentId!.Value)
				.Select(g => new { ParentId = g.Key, Count = g.Count() })
				.ToDictionaryAsync(x => x.ParentId, x => x.Count);
		}
    }
}
