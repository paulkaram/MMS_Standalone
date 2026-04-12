using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IPermissionMatrixRepository : IRepository<PermissionMatrix>
    {
        Task<List<PermissionMatrix>?> ListIncludePermission(Expression<Func<PermissionMatrix, bool>> filter);

    }
}