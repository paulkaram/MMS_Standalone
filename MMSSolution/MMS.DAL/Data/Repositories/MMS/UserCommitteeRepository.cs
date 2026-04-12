using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class UserCommitteeRepository : Repository<UserCommittee>, IUserCommitteeRepository
    {
        MmsContext ContextAsMMSContext => (Context as MmsContext)!;
        public UserCommitteeRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<UserCommittee>?> ListIncludeAllAsync(Expression<Func<UserCommittee, bool>> filter)
        {
            return await ContextAsMMSContext.UserCommittees.AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.CommitteeRole)
            .Include(x => x.Privacy)
            .Include(x => x.Committee).Where(filter).ToListAsync();
        }

        public async Task<List<UserCommittee>?> ListIncludeCommitteeAsync(Expression<Func<UserCommittee, bool>> filter)
        {
            return await ContextAsMMSContext.UserCommittees.AsNoTracking()
			.Include(x => x.Committee)
            .Where(filter).ToListAsync();
        }
        public async Task<List<UserCommittee>?> ListIncludeCommitteeRoleAsync(Expression<Func<UserCommittee, bool>> filter)
        {
            return await ContextAsMMSContext.UserCommittees.AsNoTracking()
            .Include(x => x.CommitteeRole)
            .Where(filter).ToListAsync();
        }
		public async Task<List<UserCommittee>?> ListIncludeUserAndRolePaginatedAsync(Expression<Func<UserCommittee, bool>> filter,int page,int pageSize)
		{
			return await ContextAsMMSContext.UserCommittees
			.Include(x => x.User)
			.Include(x => x.Privacy)
			.Include(x => x.CommitteeRole).AsNoTracking()
			.Where(filter).Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
		}
	}
}
