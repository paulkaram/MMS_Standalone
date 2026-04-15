using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class WorkflowInstanceRepository : Repository<WorkflowInstance>, IWorkflowInstanceRepository
    {
        private MmsContext Ctx => (Context as MmsContext)!;

        public WorkflowInstanceRepository(DbContext context) : base(context) { }

        public async Task<WorkflowInstance?> GetByBidAsync(int bidId)
        {
            return await Ctx.WorkflowInstances
                .Include(i => i.CurrentStep)
                .Include(i => i.Template).ThenInclude(t => t.Steps)
                .Include(i => i.Template).ThenInclude(t => t.Transitions)
                .FirstOrDefaultAsync(i => i.BidId == bidId);
        }

        public async Task<WorkflowInstance?> GetWithTemplateAsync(int instanceId)
        {
            return await Ctx.WorkflowInstances
                .Include(i => i.CurrentStep)
                .Include(i => i.Template).ThenInclude(t => t.Steps)
                .Include(i => i.Template).ThenInclude(t => t.Transitions)
                .FirstOrDefaultAsync(i => i.Id == instanceId);
        }
    }
}
