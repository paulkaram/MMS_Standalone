using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class CommitteeDuty
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int CommitteeId { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Committee Committee { get; set; } = null!;

    public virtual ICollection<MeetingAgendum> MeetingAgenda { get; set; } = new List<MeetingAgendum>();
}
