using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class MeetingAgendaRecommendation
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public string CreateBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string Owner { get; set; } = null!;

    public int StatusId { get; set; }

    public int Percentage { get; set; }

    public int MeetingAgendaId { get; set; }

    public DateTime? DueDate { get; set; }

    public string? OwnerStructureId { get; set; }

    public string? Description { get; set; }

    public int? PriorityId { get; set; }

    public virtual User CreateByNavigation { get; set; } = null!;

    public virtual MeetingAgendum MeetingAgenda { get; set; } = null!;

    public virtual User OwnerNavigation { get; set; } = null!;

    public virtual MeetingAgendaRecommendationStatus Status { get; set; } = null!;

    public virtual Priority? Priority { get; set; }
}
