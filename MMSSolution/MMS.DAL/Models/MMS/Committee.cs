using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class Committee
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public int TypeId { get; set; }

    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public bool Active { get; set; }

    public DateTime CreatedDate { get; set; }

    public int Sort { get; set; }

    public int? BranchId { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? CommitteeClassificationId { get; set; }

    public int? CommitteeStyleId { get; set; }

    public bool HasFinancialCompensation { get; set; }

    public bool HasAdditionalMembers { get; set; }

    public bool IsInternal { get; set; }

    public bool IsPresentationRelated { get; set; }

    public string? AdditionalMemberName { get; set; }

    public int? CommitteeStatusId { get; set; }

    public virtual ICollection<CommitteeActivity> CommitteeActivities { get; set; } = new List<CommitteeActivity>();

    public virtual CommitteeClassification? CommitteeClassification { get; set; }

    public virtual ICollection<CommitteeDuty> CommitteeDuties { get; set; } = new List<CommitteeDuty>();

    public virtual ICollection<CommitteePermission> CommitteePermissions { get; set; } = new List<CommitteePermission>();

    public virtual CommitteeStatus? CommitteeStatus { get; set; }

    public virtual CommitteeStyle? CommitteeStyle { get; set; }

    public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();

    public virtual CommitteeType Type { get; set; } = null!;

    public virtual ICollection<UserCommittee> UserCommittees { get; set; } = new List<UserCommittee>();
}
