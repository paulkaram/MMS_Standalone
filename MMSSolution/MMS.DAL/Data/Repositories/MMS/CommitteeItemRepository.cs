using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class CommitteeItemRepository : Repository<CommitteeItem>, ICommitteeItemRepository
    {
        private MmsContext ContextAsMMSContext => (Context as MmsContext)!;

        public CommitteeItemRepository(DbContext context) : base(context)
        {
        }

        public async Task<CommitteeItem?> GetIncludeRelationsAsync(Expression<Func<CommitteeItem, bool>> filter)
        {
            return await ContextAsMMSContext.Set<CommitteeItem>()
                .Include(i => i.ItemType)
                .Include(i => i.BidItemType)
                .Include(i => i.Committee)
                .Include(i => i.RelatedItem)
                .Include(i => i.CreatedByNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(filter);
        }

        public async Task<IEnumerable<CommitteeItem>> ListIncludeRelationsAsync(Expression<Func<CommitteeItem, bool>> filter)
        {
            return await ContextAsMMSContext.Set<CommitteeItem>()
                .Include(i => i.ItemType)
                .Include(i => i.BidItemType)
                .Include(i => i.RelatedItem)
                .Include(i => i.CreatedByNavigation)
                .Where(filter)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetNextSequenceAsync(int committeeId, int year)
        {
            var prefixStart = year.ToString();
            var count = await ContextAsMMSContext.Set<CommitteeItem>()
                .Where(i => i.CommitteeId == committeeId && i.ReferenceNumber.Contains("-" + prefixStart + "-"))
                .CountAsync();
            return count + 1;
        }
    }
}
