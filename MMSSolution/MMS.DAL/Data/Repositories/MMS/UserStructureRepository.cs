using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class UserStructureRepository : Repository<UserStructure>, IUserStructureRepository
	{
        MmsContext ContextAsMMSContext => (Context as MmsContext)!;
        public UserStructureRepository(DbContext context) : base(context)
		{
		}

		public async Task<List<string>> ListUserRoleIdsAsync(string userId)
		{
			return await ContextAsMMSContext.UserStructures.Where(x => x.UserId == userId).Select(x => x.RoleId.ToString()).ToListAsync();
		}

		public async Task<List<UserStructure>?> ListIncludeStructureAndRoleAsync(Expression<Func<UserStructure, bool>> filter)
		{
			return await ContextAsMMSContext.UserStructures
				.Include(x => x.Role)
				.Include(x => x.Strucutre).Where(filter).ToListAsync();
		}
		
		public async Task<List<UserStructure>?> ListIncludeAllAsync(Expression<Func<UserStructure, bool>> filter)
		{
			return await ContextAsMMSContext.UserStructures
				.Include(x => x.Role)
				.Include(x => x.User)
				.Include(x => x.Strucutre).Where(filter).ToListAsync();
		}
	}
}