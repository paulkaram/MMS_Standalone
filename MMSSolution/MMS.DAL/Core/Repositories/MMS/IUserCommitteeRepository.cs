using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IUserCommitteeRepository : IRepository<UserCommittee>
    {
        public Task<List<UserCommittee>?> ListIncludeAllAsync(Expression<Func<UserCommittee, bool>> filter);
        public Task<List<UserCommittee>?> ListIncludeCommitteeAsync(Expression<Func<UserCommittee, bool>> filter);
        public Task<List<UserCommittee>?> ListIncludeCommitteeRoleAsync(Expression<Func<UserCommittee, bool>> filter);
		public Task<List<UserCommittee>?> ListIncludeUserAndRolePaginatedAsync(Expression<Func<UserCommittee, bool>> filter, int page, int pageSize);
    }
}
