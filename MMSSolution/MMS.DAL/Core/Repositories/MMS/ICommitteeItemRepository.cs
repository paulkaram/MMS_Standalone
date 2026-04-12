using System.Linq.Expressions;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface ICommitteeItemRepository : IRepository<CommitteeItem>
    {
        Task<CommitteeItem?> GetIncludeRelationsAsync(Expression<Func<CommitteeItem, bool>> filter);
        Task<IEnumerable<CommitteeItem>> ListIncludeRelationsAsync(Expression<Func<CommitteeItem, bool>> filter);
        Task<int> GetNextSequenceAsync(int committeeId, int year);
    }
}
