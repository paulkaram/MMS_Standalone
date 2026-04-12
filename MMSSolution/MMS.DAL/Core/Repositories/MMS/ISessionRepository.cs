using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface ISessionRepository : IRepository<Session>
    {
        Task<Session?> GetIncludeItemsAsync(Expression<Func<Session, bool>> filter);
    }
}
