using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IBidItemVisionRepository : IRepository<BidItemVision>
    {
        Task<IEnumerable<BidItemVision>> ListByBidAsync(int bidId);
        Task<IEnumerable<BidItemVision>> ListForStakeholderAsync(string userId, int? bidId = null);
        Task<BidItemVision?> GetIncludeAllAsync(int id);
        Task<int> CountByBidAndStatusAsync(int bidId, int statusId);
        Task<int> CountByBidAsync(int bidId);
    }
}
