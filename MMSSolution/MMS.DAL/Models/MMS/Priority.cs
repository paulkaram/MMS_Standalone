using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class Priority
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? NameAr { get; set; }

    public string? NameEn { get; set; }

    public virtual ICollection<MeetingAgendaRecommendation> MeetingAgendaRecommendations { get; set; } = new List<MeetingAgendaRecommendation>();
}
