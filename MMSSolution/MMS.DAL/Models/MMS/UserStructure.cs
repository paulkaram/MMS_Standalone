using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class UserStructure
{
    public string UserId { get; set; } = null!;

    public int StrucutreId { get; set; }

    public int RoleId { get; set; }

    public bool IsPrimary { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual Structure Strucutre { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
