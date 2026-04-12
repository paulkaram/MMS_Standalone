using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class PermissionMatrixRepository : Repository<PermissionMatrix>, IPermissionMatrixRepository
	{
        MmsContext ContextAsMMSContext => (Context as MmsContext)!;

        public PermissionMatrixRepository(DbContext context) : base(context)
		{
		}

        public async Task<List<PermissionMatrix>?> ListIncludePermission(Expression<Func<PermissionMatrix, bool>> filter)
        {
           return await ContextAsMMSContext.PermissionMatrices.AsNoTracking().Where(filter).Include(x=>x.Permission).ToListAsync();
        }
    }
}