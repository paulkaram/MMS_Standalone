namespace MMS.DAL.Models.MMS;

public partial class WorkflowTask
{
    public int Id { get; set; }
    public int InstanceId { get; set; }
    public int StepId { get; set; }
    public int BidId { get; set; }
    public string AssignedToUserId { get; set; } = null!;
    public int StatusId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ClaimedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? CompletedBy { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedDate { get; set; }

    public virtual WorkflowInstance Instance { get; set; } = null!;
    public virtual WorkflowStep Step { get; set; } = null!;
    public virtual User AssignedTo { get; set; } = null!;
    public virtual User? CompletedByNavigation { get; set; }
}
