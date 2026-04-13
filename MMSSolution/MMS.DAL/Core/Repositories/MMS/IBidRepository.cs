using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IBidRepository : IRepository<Bid>
    {
        System.Threading.Tasks.Task<Bid?> GetIncludeAllAsync(int id);
        System.Threading.Tasks.Task<IEnumerable<Bid>> ListByCommitteeAsync(int committeeId);
        System.Threading.Tasks.Task<int> GetNextSequenceAsync(int committeeId, int year);
    }
}
