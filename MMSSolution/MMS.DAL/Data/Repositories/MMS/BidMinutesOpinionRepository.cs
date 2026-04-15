using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class BidMinutesOpinionRepository : Repository<BidMinutesOpinion>, IBidMinutesOpinionRepository
    {
        private MmsContext Ctx => (Context as MmsContext)!;
        public BidMinutesOpinionRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<BidMinutesOpinion>> ListByBidAsync(int bidId)
        {
            return await Ctx.BidMinutesOpinions
                .Include(o => o.StakeholderUser)
                .Include(o => o.ExternalMember)
                .Where(o => o.BidId == bidId)
                .ToListAsync();
        }

        public async Task<int> CountByBidAsync(int bidId)
            => await Ctx.BidMinutesOpinions.CountAsync(o => o.BidId == bidId);

        public async Task<int> CountByBidAndStatusAsync(int bidId, int statusId)
            => await Ctx.BidMinutesOpinions.CountAsync(o => o.BidId == bidId && o.StatusId == statusId);
    }
}
