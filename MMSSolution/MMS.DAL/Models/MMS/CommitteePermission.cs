using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class CommitteePermission
{
    public int Id { get; set; }

    public int CommitteeId { get; set; }

    public string UserId { get; set; } = null!;

    public int PermissionId { get; set; }

    public virtual Committee Committee { get; set; } = null!;

    public virtual Permission Permission { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
