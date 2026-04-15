using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IWorkflowInstanceRepository : IRepository<WorkflowInstance>
    {
        Task<WorkflowInstance?> GetByBidAsync(int bidId);
        Task<WorkflowInstance?> GetWithTemplateAsync(int instanceId);
    }
}
