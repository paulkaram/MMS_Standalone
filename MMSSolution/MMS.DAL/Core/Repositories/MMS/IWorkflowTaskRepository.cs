using MMS.DAL.Models.MMS;
using Task = System.Threading.Tasks.Task;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IWorkflowTaskRepository : IRepository<WorkflowTask>
    {
        Task<IEnumerable<WorkflowTask>> ListForUserAsync(string userId, bool includeCompleted = false);
        Task<IEnumerable<WorkflowTask>> ListForInstanceAsync(int instanceId);
        Task<IEnumerable<WorkflowTask>> ListPendingForStepAsync(int instanceId, int stepId);
        Task CancelPendingForInstanceAsync(int instanceId);
    }
}
