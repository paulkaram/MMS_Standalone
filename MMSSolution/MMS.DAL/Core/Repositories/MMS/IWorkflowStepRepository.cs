using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IWorkflowStepRepository : IRepository<WorkflowStep> { }
    public interface IWorkflowTransitionRepository : IRepository<WorkflowTransition>
    {
        Task<IEnumerable<WorkflowTransition>> ListByTemplateAsync(int templateId);
        Task<IEnumerable<WorkflowTransition>> ListFromStepAsync(int stepId);
    }
    public interface IWorkflowHistoryRepository : IRepository<WorkflowHistory>
    {
        Task<IEnumerable<WorkflowHistory>> ListForInstanceAsync(int instanceId);
    }
}
