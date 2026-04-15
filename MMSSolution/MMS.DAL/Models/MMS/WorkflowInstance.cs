namespace MMS.DAL.Models.MMS;

public partial class WorkflowInstance
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public int BidId { get; set; }
    public int CurrentStepId { get; set; }
    public DateTime StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string StartedBy { get; set; } = null!;

    public virtual WorkflowTemplate Template { get; set; } = null!;
    public virtual Bid Bid { get; set; } = null!;
    public virtual WorkflowStep CurrentStep { get; set; } = null!;
    public virtual User StartedByNavigation { get; set; } = null!;
    public virtual ICollection<WorkflowTask> Tasks { get; set; } = new List<WorkflowTask>();
    public virtual ICollection<WorkflowHistory> History { get; set; } = new List<WorkflowHistory>();
}
