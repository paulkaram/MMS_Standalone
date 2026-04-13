namespace MMS.DAL.Models.MMS;

public partial class ExternalMember
{
    public int Id { get; set; }

    public string FullnameAr { get; set; } = null!;

    public string FullnameEn { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Mobile { get; set; }

    public string? Organization { get; set; }

    public string? Position { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public virtual ICollection<CommitteeExternalMember> CommitteeMemberships { get; set; } = new List<CommitteeExternalMember>();

    public virtual ICollection<MeetingAttendee> MeetingAttendees { get; set; } = new List<MeetingAttendee>();
}
