namespace MMS.DAL.Models.MMS;

public partial class Session
{
    public int Id { get; set; }

    public string ReferenceNumber { get; set; } = null!;

    public string ExternalReferenceNumber { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime MeetingDate { get; set; }

    public DateTime DueDate { get; set; }

    public string? Tags { get; set; }

    public int CommitteeId { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual Committee Committee { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<SessionItem> SessionItems { get; set; } = new List<SessionItem>();
}
