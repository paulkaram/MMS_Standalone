using System;

namespace MMS.DAL.Models.MMS;

public partial class MeetingAgendaSummary
{
    public int Id { get; set; }

    public int MeetingAgendaId { get; set; }

    public string Text { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public virtual MeetingAgendum MeetingAgenda { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;
}
