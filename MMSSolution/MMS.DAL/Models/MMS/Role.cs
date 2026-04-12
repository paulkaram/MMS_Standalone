using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class Role
{
    public int Id { get; set; }

    public string RoleNameAr { get; set; } = null!;

    public string RoleNameEn { get; set; } = null!;

    public int TypeId { get; set; }

    public virtual ICollection<PermissionMatrix> PermissionMatrices { get; set; } = new List<PermissionMatrix>();

    public virtual RoleType Type { get; set; } = null!;

    public virtual ICollection<UserStructure> UserStructures { get; set; } = new List<UserStructure>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
