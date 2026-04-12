using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class PermissionRepository : Repository<Permission>, IPermissionRepository
	{
        MmsContext ContextAsMMSContext => (Context as MmsContext)!;

        public PermissionRepository(DbContext context) : base(context)
		{
		}

        public async Task<List<Permission>> ListByUserId(string userId)
        {
            return await ContextAsMMSContext.PermissionMatrices
                .Where(x => x.UserId == userId)
                .Select(x => x.Permission)
                .ToListAsync();
        }

		public async Task<List<Permission>> ListIncludeTypeAsync(Expression<Func<Permission, bool>> filter)
		{
			return await ContextAsMMSContext.Permissions
				.Include(x=> x.Type)
				.Where(filter)
				.ToListAsync();
		}

		public async Task<List<Permission>> ListIncludeTypeAsync()
		{
			return await ContextAsMMSContext.Permissions
				.Include(x => x.Type)
				.ToListAsync();
		}

		public async Task<int?> GetPermissionLevelByUserIdAndPermission(string userId, int permissionId)
		{
			var permissionMatrix =  await ContextAsMMSContext.PermissionMatrices.FirstOrDefaultAsync(x=> x.UserId == userId && x.PermissionId == permissionId);
			if (permissionMatrix != null)
			{
				return permissionMatrix.LevelId;
			}
			return null;
		}
	}
}
