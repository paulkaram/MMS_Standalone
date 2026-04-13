using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class BidRepository : Repository<Bid>, IBidRepository
    {
        private MmsContext ContextAsMMSContext => (Context as MmsContext)!;

        public BidRepository(DbContext context) : base(context)
        {
        }

        public async System.Threading.Tasks.Task<Bid?> GetIncludeAllAsync(int id)
        {
            return await ContextAsMMSContext.Set<Bid>()
                .Include(b => b.Committee)
                .Include(b => b.Status)
                .Include(b => b.TeamLeader)
                .Include(b => b.CreatedByNavigation)
                .Include(b => b.Stakeholders).ThenInclude(s => s.User)
                .Include(b => b.Stakeholders).ThenInclude(s => s.ExternalMember)
                .Include(b => b.StatusHistory).ThenInclude(h => h.FromStatus)
                .Include(b => b.StatusHistory).ThenInclude(h => h.ToStatus)
                .Include(b => b.StatusHistory).ThenInclude(h => h.ChangedByNavigation)
                .Include(b => b.Items).ThenInclude(i => i.ItemType)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async System.Threading.Tasks.Task<IEnumerable<Bid>> ListByCommitteeAsync(int committeeId)
        {
            return await ContextAsMMSContext.Set<Bid>()
                .Include(b => b.Status)
                .Include(b => b.TeamLeader)
                .Where(b => b.CommitteeId == committeeId)
                .OrderByDescending(b => b.CreatedDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task<int> GetNextSequenceAsync(int committeeId, int year)
        {
            var yearFragment = "-" + year + "-";
            var count = await ContextAsMMSContext.Set<Bid>()
                .Where(b => b.CommitteeId == committeeId && b.ReferenceNumber.Contains(yearFragment))
                .CountAsync();
            return count + 1;
        }
    }
}
