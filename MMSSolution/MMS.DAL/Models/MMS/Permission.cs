using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class Permission
{
    public int Id { get; set; }

    public int TypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsSpecific { get; set; }

    public int? Order { get; set; }

    public bool? ShowLevel { get; set; }

    public string? GroupName { get; set; }

    public int? GroupItemOrder { get; set; }

    public bool IsDefault { get; set; }

    public int? MapId { get; set; }

    public virtual ICollection<CommitteePermission> CommitteePermissions { get; set; } = new List<CommitteePermission>();

    public virtual ICollection<PermissionMatrix> PermissionMatrices { get; set; } = new List<PermissionMatrix>();

    public virtual PermissionType Type { get; set; } = null!;
}
