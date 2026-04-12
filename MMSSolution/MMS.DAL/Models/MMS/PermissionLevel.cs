using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class PermissionLevel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PermissionMatrix> PermissionMatrices { get; set; } = new List<PermissionMatrix>();
}
