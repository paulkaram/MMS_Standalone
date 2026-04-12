using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class CommitteeActivityRepository : Repository<CommitteeActivity>, ICommitteeActivityRepository
    {
        MmsContext ContextAsMMSContext => (Context as MmsContext)!;
        public CommitteeActivityRepository(DbContext context) : base(context)
        {
        }
        public async Task<List<CommitteeActivity>> ListIncludeCommitteeAsync(Expression<Func<CommitteeActivity, bool>> filter)
        {
            return await ContextAsMMSContext.CommitteeActivities
                .Include(x => x.Committee)
                .Where(filter).ToListAsync();
        }

        public async Task<List<CommitteeActivity>?> ListIncludeActivityPaginatedAsync(Expression<Func<CommitteeActivity, bool>> filter, int page, int pageSize)
        {
            return await ContextAsMMSContext.CommitteeActivities
            .Where(x=>x.IsDeleted!=true)
            .Where(filter).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

    }
}
