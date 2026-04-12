using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class RoleType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
