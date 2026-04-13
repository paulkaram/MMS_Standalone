using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class BidStatusRepository : Repository<BidStatus>, IBidStatusRepository
    {
        public BidStatusRepository(DbContext context) : base(context)
        {
        }
    }
}
