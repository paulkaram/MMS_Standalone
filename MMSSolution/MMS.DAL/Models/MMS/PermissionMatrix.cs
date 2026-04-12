using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class PermissionMatrix
{
    public int Id { get; set; }

    public int PermissionId { get; set; }

    public string? UserId { get; set; }

    public int? StructureId { get; set; }

    public int? RoleId { get; set; }

    public int LevelId { get; set; }

    public bool Value { get; set; }

    public int? RecordId { get; set; }

    public bool? Everyone { get; set; }

    public virtual PermissionLevel Level { get; set; } = null!;

    public virtual Permission Permission { get; set; } = null!;

    public virtual Role? Role { get; set; }

    public virtual Structure? Structure { get; set; }

    public virtual User? User { get; set; }
}
