using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class MeetingAttendee
{
    public int Id { get; set; }

    public int MeetingId { get; set; }

    public string UserId { get; set; } = null!;

    public bool NeedsApproval { get; set; }

    public bool Attended { get; set; }

    public string? JobTitle { get; set; }

    public virtual Meeting Meeting { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
