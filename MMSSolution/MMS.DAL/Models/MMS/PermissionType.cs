using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class PermissionType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
