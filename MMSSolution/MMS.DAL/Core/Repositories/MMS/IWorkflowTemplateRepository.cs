using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IWorkflowTemplateRepository : IRepository<WorkflowTemplate>
    {
        Task<WorkflowTemplate?> GetIncludeAllAsync(int id);
        Task<IEnumerable<WorkflowTemplate>> ListAllAsync();
        Task<WorkflowTemplate?> GetForCommitteeAsync(int committeeId);
        Task<WorkflowTemplate?> GetGlobalDefaultAsync();
    }
}
