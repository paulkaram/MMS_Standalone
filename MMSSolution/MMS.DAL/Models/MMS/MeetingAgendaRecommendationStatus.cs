using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class MeetingAgendaRecommendationStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public virtual ICollection<MeetingAgendaRecommendation> MeetingAgendaRecommendations { get; set; } = new List<MeetingAgendaRecommendation>();
}
