using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class CommitteeDutyRepository : Repository<CommitteeDuty>, ICommitteeDutyRepository
    {
        MmsContext ContextAsMMSContext => (Context as MmsContext)!;
        public CommitteeDutyRepository(DbContext context) : base(context)
        {
        }
        public async Task<List<CommitteeDuty>> ListIncludeCommitteeAsync(Expression<Func<CommitteeDuty, bool>> filter)
        {
            return await ContextAsMMSContext.CommitteeDuties
                .Include(x => x.Committee)
                .Where(filter).ToListAsync();
        }
        
    }
}
