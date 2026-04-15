using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IProcurementProjectRepository : IRepository<ProcurementProject>
    {
        Task<ProcurementProject?> GetIncludeAllAsync(int id);
        Task<IEnumerable<ProcurementProject>> ListAllAsync();
    }

    public interface ICompetitorRepository : IRepository<Competitor>
    {
        Task<IEnumerable<Competitor>> ListByProjectAsync(int projectId);
    }

    public interface ICompetitorAttachmentRepository : IRepository<CompetitorAttachment>
    {
        Task<IEnumerable<CompetitorAttachment>> ListByCompetitorAsync(int competitorId);
    }

    public interface IBidItemTypeRepository : IRepository<BidItemType> { }
}
