namespace MMS.DAL.Models.MMS;

public partial class WorkflowTransition
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public int FromStepId { get; set; }
    public int ToStepId { get; set; }
    public string LabelAr { get; set; } = null!;
    public string LabelEn { get; set; } = null!;
    public int ActionType { get; set; }
    public int DisplayOrder { get; set; }

    public virtual WorkflowTemplate Template { get; set; } = null!;
    public virtual WorkflowStep FromStep { get; set; } = null!;
    public virtual WorkflowStep ToStep { get; set; } = null!;
}
