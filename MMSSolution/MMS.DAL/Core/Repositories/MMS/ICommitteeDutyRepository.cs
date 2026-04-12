using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface ICommitteeDutyRepository : IRepository<CommitteeDuty>
    {
        Task<List<CommitteeDuty>> ListIncludeCommitteeAsync(Expression<Func<CommitteeDuty, bool>> filter);
    }
}
