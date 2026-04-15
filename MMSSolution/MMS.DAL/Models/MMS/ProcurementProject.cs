namespace MMS.DAL.Models.MMS;

public partial class ProcurementProject
{
    public int Id { get; set; }
    public string PurchaseOrderNumber { get; set; } = null!;
    public string ProjectName { get; set; } = null!;
    public string? ProjectManagerUserId { get; set; }
    public decimal? EstimatedValue { get; set; }
    public int AttachmentMode { get; set; }     // ProcurementAttachmentModeDbEnum
    public DateTime? MeetingDate { get; set; }
    public string? MeetingLocation { get; set; }
    public int StatusId { get; set; }
    public int? CommitteeId { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public virtual User? ProjectManager { get; set; }
    public virtual Committee? Committee { get; set; }
    public virtual User CreatedByNavigation { get; set; } = null!;
    public virtual ICollection<Competitor> Competitors { get; set; } = new List<Competitor>();
}

public partial class Competitor
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string? SapCompanyId { get; set; }
    public string CompanyName { get; set; } = null!;
    public string? CommercialRegistrationNumber { get; set; }
    public bool IsDataComplete { get; set; }
    public decimal? FinancialValue { get; set; }
    public bool HasBankGuarantee { get; set; }
    public bool HasSmeLicense { get; set; }
    public DateTime CreatedDate { get; set; }

    public virtual ProcurementProject Project { get; set; } = null!;
    public virtual ICollection<CompetitorAttachment> Attachments { get; set; } = new List<CompetitorAttachment>();
}

public partial class CompetitorAttachment
{
    public int Id { get; set; }
    public int CompetitorId { get; set; }
    public int AttachmentId { get; set; }
    public int Category { get; set; }   // CompetitorAttachmentCategoryDbEnum

    public virtual Competitor Competitor { get; set; } = null!;
    public virtual Attachment Attachment { get; set; } = null!;
}
