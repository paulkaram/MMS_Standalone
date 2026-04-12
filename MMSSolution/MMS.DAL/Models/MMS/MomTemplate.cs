namespace MMS.DAL.Models.MMS;

public partial class MomTemplate
{
    public int Id { get; set; }

    public int? BranchId { get; set; }

    public int TemplateType { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string ConfigJson { get; set; } = null!;

    /// <summary>
    /// HTML template with placeholders for generating PDF
    /// Placeholders: {{Title}}, {{Date}}, {{Attendees}}, etc.
    /// </summary>
    public string? HtmlTemplate { get; set; }

    public bool IsActive { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual Branch? Branch { get; set; }
}
