using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class AgendaRecommendation
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public string CreateBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string Owner { get; set; } = null!;

    public int StatusId { get; set; }

    public int Percentage { get; set; }

    public virtual User CreateByNavigation { get; set; } = null!;

    public virtual User OwnerNavigation { get; set; } = null!;

    public virtual AgendaRecommendationStatus Status { get; set; } = null!;
}
