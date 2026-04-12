using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class SessionItemTypeRepository : Repository<SessionItemType>, ISessionItemTypeRepository
    {
        public SessionItemTypeRepository(DbContext context) : base(context)
        {
        }
    }
}
