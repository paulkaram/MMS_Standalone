using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class WorkflowStepRepository : Repository<WorkflowStep>, IWorkflowStepRepository
    {
        public WorkflowStepRepository(DbContext context) : base(context) { }
    }

    internal class WorkflowTransitionRepository : Repository<WorkflowTransition>, IWorkflowTransitionRepository
    {
        private MmsContext Ctx => (Context as MmsContext)!;
        public WorkflowTransitionRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<WorkflowTransition>> ListByTemplateAsync(int templateId)
        {
            return await Ctx.WorkflowTransitions.Where(t => t.TemplateId == templateId).ToListAsync();
        }

        public async Task<IEnumerable<WorkflowTransition>> ListFromStepAsync(int stepId)
        {
            return await Ctx.WorkflowTransitions
                .Include(t => t.ToStep)
                .Where(t => t.FromStepId == stepId)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();
        }
    }

    internal class WorkflowHistoryRepository : Repository<WorkflowHistory>, IWorkflowHistoryRepository
    {
        private MmsContext Ctx => (Context as MmsContext)!;
        public WorkflowHistoryRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<WorkflowHistory>> ListForInstanceAsync(int instanceId)
        {
            return await Ctx.WorkflowHistory
                .Include(h => h.FromStep)
                .Include(h => h.ToStep)
                .Include(h => h.Transition)
                .Include(h => h.ChangedByNavigation)
                .Where(h => h.InstanceId == instanceId)
                .OrderByDescending(h => h.ChangedDate)
                .ToListAsync();
        }
    }
}
