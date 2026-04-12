using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class AgendaRecommendationStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<AgendaRecommendation> AgendaRecommendations { get; set; } = new List<AgendaRecommendation>();
}
