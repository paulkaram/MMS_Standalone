namespace MMS.DAL.Models.MMS;

public partial class CommitteeExternalMember
{
    public int Id { get; set; }

    public int CommitteeId { get; set; }

    public int ExternalMemberId { get; set; }

    public int CommitteeRoleId { get; set; }

    public bool Active { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Committee Committee { get; set; } = null!;

    public virtual ExternalMember ExternalMember { get; set; } = null!;

    public virtual CommitteeRole CommitteeRole { get; set; } = null!;
}
