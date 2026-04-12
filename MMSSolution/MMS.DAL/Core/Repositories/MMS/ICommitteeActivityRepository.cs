using MMS.DAL.Models.MMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface ICommitteeActivityRepository : IRepository<CommitteeActivity>
    {
        Task<List<CommitteeActivity>> ListIncludeCommitteeAsync(Expression<Func<CommitteeActivity, bool>> filter);
        Task<List<CommitteeActivity>?> ListIncludeActivityPaginatedAsync(Expression<Func<CommitteeActivity, bool>> filter, int page, int pageSize);
    }
}
