using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class CommitteeExternalMemberRepository : Repository<CommitteeExternalMember>, ICommitteeExternalMemberRepository
    {
        private MmsContext ContextAsMMSContext => (Context as MmsContext)!;

        public CommitteeExternalMemberRepository(DbContext context) : base(context)
        {
        }

        public async System.Threading.Tasks.Task<IEnumerable<CommitteeExternalMember>> ListByCommitteeAsync(int committeeId)
        {
            return await ContextAsMMSContext.Set<CommitteeExternalMember>()
                .Include(c => c.ExternalMember)
                .Include(c => c.CommitteeRole)
                .Where(c => c.CommitteeId == committeeId && c.Active)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
