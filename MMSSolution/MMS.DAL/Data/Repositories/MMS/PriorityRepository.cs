using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class PriorityRepository : Repository<Priority>, IPriorityRepository
    {
        public PriorityRepository(DbContext context) : base(context)
        {
        }
    }
}
