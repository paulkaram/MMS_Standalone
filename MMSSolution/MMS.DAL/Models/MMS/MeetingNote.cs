using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class MeetingNote
{
    public int Id { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? Text { get; set; }

    public int MeetingId { get; set; }

    public int TaskId { get; set; }

    public virtual Meeting Meeting { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
