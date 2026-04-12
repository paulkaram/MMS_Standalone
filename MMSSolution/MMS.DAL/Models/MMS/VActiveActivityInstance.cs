using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class VActiveActivityInstance
{
    public int ActivityInstanceId { get; set; }

    public int? WorkflowInstanceId { get; set; }

    public DateTime PlannedStartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public int IsDelayed { get; set; }

    public string? OwnerId { get; set; }

    public string? OwnerAr { get; set; }

    public string? OwnerEn { get; set; }

    public string? UserId { get; set; }

    public string? Username { get; set; }

    public string? UserFullnameAr { get; set; }

    public string? UserFullnameEn { get; set; }

    public string? RoleNameEn { get; set; }

    public string? RoleNameAr { get; set; }

    public int? RoleId { get; set; }

    public string? StructureNameAr { get; set; }

    public string? StructureNameEn { get; set; }

    public int? StructureId { get; set; }

    public int? Priority { get; set; }

    public string? Title { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Initiator { get; set; }

    public string? ProcessTitle { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool? Claimed { get; set; }
}
