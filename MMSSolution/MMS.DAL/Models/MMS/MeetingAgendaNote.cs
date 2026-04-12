using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class MeetingAgendaNote
{
    public int Id { get; set; }

    public int MeetingAgendaId { get; set; }

    public string UserId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? Text { get; set; }

    public bool IsPublic { get; set; }

    public virtual MeetingAgendum MeetingAgenda { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
