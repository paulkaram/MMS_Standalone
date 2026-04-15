using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IBidMinutesOpinionRepository : IRepository<BidMinutesOpinion>
    {
        Task<IEnumerable<BidMinutesOpinion>> ListByBidAsync(int bidId);
        Task<int> CountByBidAsync(int bidId);
        Task<int> CountByBidAndStatusAsync(int bidId, int statusId);
    }
}
