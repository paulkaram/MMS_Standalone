using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IDelegationRepository : IRepository<Delegation>
    {
        System.Threading.Tasks.Task<IEnumerable<Delegation>> ListByFromUserAsync(string userId);
        System.Threading.Tasks.Task<IEnumerable<Delegation>> ListByToUserAsync(string userId);
        System.Threading.Tasks.Task<IEnumerable<Delegation>> ListActiveGeneralForToUserAsync(string toUserId, DateTime at);
        System.Threading.Tasks.Task<bool> HasOverlappingGeneralAsync(string fromUserId, DateTime start, DateTime end, int? excludeId);
        System.Threading.Tasks.Task<Delegation?> GetIncludeRelationsAsync(int id);
    }
}
