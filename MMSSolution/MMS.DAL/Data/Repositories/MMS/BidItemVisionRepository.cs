using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class BidItemVisionRepository : Repository<BidItemVision>, IBidItemVisionRepository
    {
        private MmsContext ContextAsMMSContext => (Context as MmsContext)!;

        public BidItemVisionRepository(DbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<BidItemVision>> ListByBidAsync(int bidId)
        {
            return await ContextAsMMSContext.Set<BidItemVision>()
                .Include(v => v.BidItem)
                .Include(v => v.StakeholderUser)
                .Include(v => v.ExternalMember)
                .Where(v => v.BidId == bidId)
                .ToListAsync();
        }

        public async Task<IEnumerable<BidItemVision>> ListForStakeholderAsync(string userId, int? bidId = null)
        {
            var query = ContextAsMMSContext.Set<BidItemVision>()
                .Include(v => v.Bid)
                .Include(v => v.BidItem)
                .Where(v => v.StakeholderUserId == userId);

            if (bidId.HasValue)
                query = query.Where(v => v.BidId == bidId.Value);

            return await query.ToListAsync();
        }

        public async Task<BidItemVision?> GetIncludeAllAsync(int id)
        {
            return await ContextAsMMSContext.Set<BidItemVision>()
                .Include(v => v.Bid)
                .Include(v => v.BidItem)
                .Include(v => v.StakeholderUser)
                .Include(v => v.ExternalMember)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<int> CountByBidAndStatusAsync(int bidId, int statusId)
        {
            return await ContextAsMMSContext.Set<BidItemVision>()
                .CountAsync(v => v.BidId == bidId && v.StatusId == statusId);
        }

        public async Task<int> CountByBidAsync(int bidId)
        {
            return await ContextAsMMSContext.Set<BidItemVision>()
                .CountAsync(v => v.BidId == bidId);
        }
    }
}
