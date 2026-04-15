namespace MMS.DTO.Procurement
{
    public class ProcurementProjectDto
    {
        public int Id { get; set; }
        public string PurchaseOrderNumber { get; set; } = null!;
        public string ProjectName { get; set; } = null!;
        public string? ProjectManagerUserId { get; set; }
        public string? ProjectManagerName { get; set; }
        public decimal? EstimatedValue { get; set; }
        public int AttachmentMode { get; set; }
        public string AttachmentModeName { get; set; } = null!;
        public DateTime? MeetingDate { get; set; }
        public string? MeetingLocation { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } = null!;
        public int? CommitteeId { get; set; }
        public string? CommitteeName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CompetitorsCount { get; set; }
        public List<CompetitorDto> Competitors { get; set; } = new();
    }

    public class ProcurementProjectPostDto
    {
        public string PurchaseOrderNumber { get; set; } = null!;
        public string ProjectName { get; set; } = null!;
        public string? ProjectManagerUserId { get; set; }
        public decimal? EstimatedValue { get; set; }
        public int AttachmentMode { get; set; } = 1;   // SingleFile default
        public DateTime? MeetingDate { get; set; }
        public string? MeetingLocation { get; set; }
        public int? CommitteeId { get; set; }
    }

    public class CompetitorDto
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
        public List<CompetitorAttachmentDto> Attachments { get; set; } = new();
    }

    public class CompetitorPostDto
    {
        public string? SapCompanyId { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? CommercialRegistrationNumber { get; set; }
        public bool IsDataComplete { get; set; }
        public decimal? FinancialValue { get; set; }
        public bool HasBankGuarantee { get; set; }
        public bool HasSmeLicense { get; set; }
    }

    public class CompetitorAttachmentDto
    {
        public int Id { get; set; }
        public int CompetitorId { get; set; }
        public int AttachmentId { get; set; }
        public int Category { get; set; }
        public string CategoryName { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public int FileSize { get; set; }
    }

    /// <summary>
    /// Stub response from the ERP lookup endpoint. Real integration (§5.11 + #29)
    /// will replace this with a call to the external ERP system.
    /// </summary>
    public class ErpProjectLookupDto
    {
        public string PurchaseOrderNumber { get; set; } = null!;
        public string ProjectName { get; set; } = null!;
        public string? ProjectManagerUserId { get; set; }
        public string? ProjectManagerName { get; set; }
        public decimal? EstimatedValue { get; set; }
        public bool Found { get; set; }
        public string? Note { get; set; }
    }
}
