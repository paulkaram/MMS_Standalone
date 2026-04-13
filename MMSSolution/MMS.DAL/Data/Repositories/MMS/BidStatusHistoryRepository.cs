using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class BidStatusHistoryRepository : Repository<BidStatusHistory>, IBidStatusHistoryRepository
    {
        public BidStatusHistoryRepository(DbContext context) : base(context)
        {
        }
    }
}
