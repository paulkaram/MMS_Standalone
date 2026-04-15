using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class WorkflowTemplateRepository : Repository<WorkflowTemplate>, IWorkflowTemplateRepository
    {
        private MmsContext Ctx => (Context as MmsContext)!;

        public WorkflowTemplateRepository(DbContext context) : base(context) { }

        public async Task<WorkflowTemplate?> GetIncludeAllAsync(int id)
        {
            return await Ctx.WorkflowTemplates
                .Include(t => t.Committee)
                .Include(t => t.Steps.OrderBy(s => s.StepOrder))
                .Include(t => t.Transitions)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<WorkflowTemplate>> ListAllAsync()
        {
            return await Ctx.WorkflowTemplates
                .Include(t => t.Committee)
                .OrderBy(t => t.CommitteeId.HasValue ? 1 : 0)
                .ThenBy(t => t.NameEn)
                .ToListAsync();
        }

        public async Task<WorkflowTemplate?> GetForCommitteeAsync(int committeeId)
        {
            return await Ctx.WorkflowTemplates
                .Include(t => t.Steps.OrderBy(s => s.StepOrder))
                .Include(t => t.Transitions)
                .FirstOrDefaultAsync(t => t.IsActive && t.CommitteeId == committeeId);
        }

        public async Task<WorkflowTemplate?> GetGlobalDefaultAsync()
        {
            return await Ctx.WorkflowTemplates
                .Include(t => t.Steps.OrderBy(s => s.StepOrder))
                .Include(t => t.Transitions)
                .FirstOrDefaultAsync(t => t.IsActive && t.CommitteeId == null);
        }
    }
}
