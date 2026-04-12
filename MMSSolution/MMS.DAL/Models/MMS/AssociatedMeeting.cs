using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class AssociatedMeeting
{
    public int Id { get; set; }

    public int MeetingId { get; set; }

    public int AssociatedId { get; set; }

    public virtual Meeting Associated { get; set; } = null!;

    public virtual Meeting Meeting { get; set; } = null!;
}
