namespace MMS.DAL.Models.MMS;

public partial class WorkflowTemplate
{
    public int Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public int? CommitteeId { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public int? InitiatorActorSourceType { get; set; }
    public string? InitiatorActorTargetId { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public virtual Committee? Committee { get; set; }
    public virtual User CreatedByNavigation { get; set; } = null!;
    public virtual ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    public virtual ICollection<WorkflowTransition> Transitions { get; set; } = new List<WorkflowTransition>();
}
