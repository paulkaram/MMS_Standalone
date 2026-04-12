using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class SessionRepository : Repository<Session>, ISessionRepository
    {
        MmsContext ContextAsMMSContext => (Context as MmsContext)!;

        public SessionRepository(DbContext context) : base(context)
        {
        }

        public async Task<Session?> GetIncludeItemsAsync(Expression<Func<Session, bool>> filter)
        {
            return await ContextAsMMSContext.Sessions
                .Include(x => x.SessionItems).ThenInclude(x => x.ItemType)
                .Include(x => x.SessionItems).ThenInclude(x => x.RelatedSessionItem)
                .Include(x => x.Committee)
                .Include(x => x.CreatedByNavigation)
                .FirstOrDefaultAsync(filter);
        }
    }
}
