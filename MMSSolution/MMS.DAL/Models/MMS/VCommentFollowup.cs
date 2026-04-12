using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class VCommentFollowup
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Text { get; set; } = null!;

    public int PlanTaskId { get; set; }

    public string CreatedByAr { get; set; } = null!;

    public string CreatedByEn { get; set; } = null!;

    public string? FollowupOwnerAr { get; set; }

    public string? FollowupOwnerEn { get; set; }

    public string ProcessTitle { get; set; } = null!;

    public DateTime ProcessStartDate { get; set; }

    public DateTime? EstimatedProcessingDate { get; set; }

    public string? PlanTaskDescription { get; set; }

    public string ResponsibleStructureAr { get; set; } = null!;

    public string ResponsibleStructureEn { get; set; } = null!;

    public string StructureAr { get; set; } = null!;

    public string StructureEn { get; set; } = null!;
}
