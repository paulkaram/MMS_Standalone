namespace MMS.DAL.Models.MMS;

public partial class WorkflowHistory
{
    public int Id { get; set; }
    public int InstanceId { get; set; }
    public int? FromStepId { get; set; }
    public int ToStepId { get; set; }
    public int? TransitionId { get; set; }
    public string ChangedBy { get; set; } = null!;
    public DateTime ChangedDate { get; set; }
    public string? Note { get; set; }

    public virtual WorkflowInstance Instance { get; set; } = null!;
    public virtual WorkflowStep? FromStep { get; set; }
    public virtual WorkflowStep ToStep { get; set; } = null!;
    public virtual WorkflowTransition? Transition { get; set; }
    public virtual User ChangedByNavigation { get; set; } = null!;
}
