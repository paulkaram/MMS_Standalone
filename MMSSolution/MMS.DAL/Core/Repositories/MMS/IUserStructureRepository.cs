using MMS.DAL.Models.MMS;
namespace MMS.DAL.Core.Repositories.MMS
{
	public interface IUserStructureRepository : IRepository<UserStructure>
	{
		Task<List<string>> ListUserRoleIdsAsync(string userId);

		public Task<List<UserStructure>?> ListIncludeStructureAndRoleAsync(System.Linq.Expressions.Expression<Func<UserStructure, bool>> filter);

        public Task<List<UserStructure>?> ListIncludeAllAsync(System.Linq.Expressions.Expression<Func<UserStructure, bool>> filter);
    }
}