using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class AgendaTopic
{
    public int Id { get; set; }

    public int MeetingAgendaId { get; set; }

    public string Text { get; set; } = null!;

    public virtual MeetingAgendum MeetingAgenda { get; set; } = null!;
}
