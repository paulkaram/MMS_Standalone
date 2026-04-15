using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class ProcurementProjectRepository : Repository<ProcurementProject>, IProcurementProjectRepository
    {
        private MmsContext Ctx => (Context as MmsContext)!;
        public ProcurementProjectRepository(DbContext context) : base(context) { }

        public async Task<ProcurementProject?> GetIncludeAllAsync(int id)
        {
            return await Ctx.ProcurementProjects
                .Include(p => p.ProjectManager)
                .Include(p => p.Committee)
                .Include(p => p.Competitors).ThenInclude(c => c.Attachments).ThenInclude(ca => ca.Attachment)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<ProcurementProject>> ListAllAsync()
        {
            return await Ctx.ProcurementProjects
                .Include(p => p.ProjectManager)
                .Include(p => p.Committee)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }
    }

    internal class CompetitorRepository : Repository<Competitor>, ICompetitorRepository
    {
        private MmsContext Ctx => (Context as MmsContext)!;
        public CompetitorRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<Competitor>> ListByProjectAsync(int projectId)
        {
            return await Ctx.Competitors
                .Include(c => c.Attachments).ThenInclude(ca => ca.Attachment)
                .Where(c => c.ProjectId == projectId)
                .ToListAsync();
        }
    }

    internal class CompetitorAttachmentRepository : Repository<CompetitorAttachment>, ICompetitorAttachmentRepository
    {
        private MmsContext Ctx => (Context as MmsContext)!;
        public CompetitorAttachmentRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<CompetitorAttachment>> ListByCompetitorAsync(int competitorId)
        {
            return await Ctx.CompetitorAttachments
                .Include(ca => ca.Attachment)
                .Where(ca => ca.CompetitorId == competitorId)
                .ToListAsync();
        }
    }

    internal class BidItemTypeRepository : Repository<BidItemType>, IBidItemTypeRepository
    {
        public BidItemTypeRepository(DbContext context) : base(context) { }
    }
}
