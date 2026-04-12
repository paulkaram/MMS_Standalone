using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class CommitteeMember
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int CommitteeId { get; set; }

    public virtual Committee Committee { get; set; } = null!;
}
