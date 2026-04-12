using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class UserCommittee
{
    public string UserId { get; set; } = null!;

    public int CommitteeId { get; set; }

    public int CommitteeRoleId { get; set; }

    public bool Active { get; set; }

    public string? Note { get; set; }

    public short PrivacyId { get; set; }

    public virtual Committee Committee { get; set; } = null!;

    public virtual CommitteeRole CommitteeRole { get; set; } = null!;

    public virtual Privacy Privacy { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
