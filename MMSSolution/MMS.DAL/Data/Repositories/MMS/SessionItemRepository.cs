using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class SessionItemRepository : Repository<SessionItem>, ISessionItemRepository
    {
        public SessionItemRepository(DbContext context) : base(context)
        {
        }
    }
}
