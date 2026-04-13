using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class BidStakeholderRepository : Repository<BidStakeholder>, IBidStakeholderRepository
    {
        private MmsContext ContextAsMMSContext => (Context as MmsContext)!;

        public BidStakeholderRepository(DbContext context) : base(context)
        {
        }

        public async System.Threading.Tasks.Task RemoveAllForBidAsync(int bidId)
        {
            var existing = await ContextAsMMSContext.Set<BidStakeholder>()
                .Where(s => s.BidId == bidId)
                .ToListAsync();
            if (existing.Any())
            {
                ContextAsMMSContext.Set<BidStakeholder>().RemoveRange(existing);
            }
        }
    }
}
