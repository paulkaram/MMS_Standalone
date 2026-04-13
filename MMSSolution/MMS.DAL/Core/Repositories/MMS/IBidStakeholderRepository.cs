using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IBidStakeholderRepository : IRepository<BidStakeholder>
    {
        System.Threading.Tasks.Task RemoveAllForBidAsync(int bidId);
    }
}
