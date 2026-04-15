namespace MMS.DAL.Models.MMS;

public partial class WorkflowStep
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public int StepOrder { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public bool IsTerminal { get; set; }
    public bool IsAutoAdvance { get; set; }
    public int ActorSourceType { get; set; }
    public string? ActorTargetId { get; set; }
    public string? TaskTitleAr { get; set; }
    public string? TaskTitleEn { get; set; }
    public string? TaskBodyAr { get; set; }
    public string? TaskBodyEn { get; set; }
    public int? SlaDays { get; set; }
    public int? LegacyBidStatusId { get; set; }

    public virtual WorkflowTemplate Template { get; set; } = null!;
    public virtual BidStatus? LegacyBidStatus { get; set; }
}
