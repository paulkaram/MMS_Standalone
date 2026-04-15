using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using Task = System.Threading.Tasks.Task;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class WorkflowTaskRepository : Repository<WorkflowTask>, IWorkflowTaskRepository
    {
        private MmsContext Ctx => (Context as MmsContext)!;

        public WorkflowTaskRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<WorkflowTask>> ListForUserAsync(string userId, bool includeCompleted = false)
        {
            var query = Ctx.WorkflowTasks
                .Include(t => t.Step)
                .Include(t => t.Instance).ThenInclude(i => i.Bid).ThenInclude(b => b.Committee)
                .Where(t => t.AssignedToUserId == userId);

            if (!includeCompleted)
            {
                query = query.Where(t => t.StatusId != (int)WorkflowTaskStatusDbEnum.Completed
                                      && t.StatusId != (int)WorkflowTaskStatusDbEnum.Cancelled);
            }

            return await query.OrderByDescending(t => t.CreatedDate).ToListAsync();
        }

        public async Task<IEnumerable<WorkflowTask>> ListForInstanceAsync(int instanceId)
        {
            return await Ctx.WorkflowTasks
                .Include(t => t.Step)
                .Include(t => t.AssignedTo)
                .Where(t => t.InstanceId == instanceId)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkflowTask>> ListPendingForStepAsync(int instanceId, int stepId)
        {
            return await Ctx.WorkflowTasks
                .Where(t => t.InstanceId == instanceId
                         && t.StepId == stepId
                         && (t.StatusId == (int)WorkflowTaskStatusDbEnum.Pending
                          || t.StatusId == (int)WorkflowTaskStatusDbEnum.InProgress))
                .ToListAsync();
        }

        public async Task CancelPendingForInstanceAsync(int instanceId)
        {
            var open = await Ctx.WorkflowTasks
                .Where(t => t.InstanceId == instanceId
                         && (t.StatusId == (int)WorkflowTaskStatusDbEnum.Pending
                          || t.StatusId == (int)WorkflowTaskStatusDbEnum.InProgress))
                .ToListAsync();

            foreach (var t in open)
            {
                t.StatusId = (int)WorkflowTaskStatusDbEnum.Cancelled;
            }
        }
    }
}
