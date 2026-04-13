using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class DelegationRepository : Repository<Delegation>, IDelegationRepository
    {
        private MmsContext ContextAsMMSContext => (Context as MmsContext)!;

        public DelegationRepository(DbContext context) : base(context)
        {
        }

        public async System.Threading.Tasks.Task<IEnumerable<Delegation>> ListByFromUserAsync(string userId)
        {
            return await ContextAsMMSContext.Set<Delegation>()
                .Include(d => d.ToUser)
                .Include(d => d.DelegationTasks)
                .Where(d => d.FromUserId == userId)
                .OrderByDescending(d => d.CreatedDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task<IEnumerable<Delegation>> ListByToUserAsync(string userId)
        {
            return await ContextAsMMSContext.Set<Delegation>()
                .Include(d => d.FromUser)
                .Include(d => d.DelegationTasks)
                .Where(d => d.ToUserId == userId)
                .OrderByDescending(d => d.CreatedDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task<IEnumerable<Delegation>> ListActiveGeneralForToUserAsync(string toUserId, DateTime at)
        {
            var generalType = (int)DelegationTypeDbEnum.General;
            return await ContextAsMMSContext.Set<Delegation>()
                .Where(d => d.ToUserId == toUserId
                         && d.TypeId == generalType
                         && d.IsActive
                         && d.StartDate <= at
                         && d.EndDate >= at)
                .AsNoTracking()
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task<bool> HasOverlappingGeneralAsync(string fromUserId, DateTime start, DateTime end, int? excludeId)
        {
            var generalType = (int)DelegationTypeDbEnum.General;
            return await ContextAsMMSContext.Set<Delegation>()
                .AnyAsync(d => d.FromUserId == fromUserId
                            && d.TypeId == generalType
                            && d.IsActive
                            && (excludeId == null || d.Id != excludeId)
                            && d.StartDate <= end
                            && d.EndDate >= start);
        }

        public async System.Threading.Tasks.Task<Delegation?> GetIncludeRelationsAsync(int id)
        {
            return await ContextAsMMSContext.Set<Delegation>()
                .Include(d => d.FromUser)
                .Include(d => d.ToUser)
                .Include(d => d.DelegationTasks)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}
