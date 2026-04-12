using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class CommitteeClassification
{
    public int Id { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string? Description { get; set; }

    public bool Active { get; set; }

    public virtual ICollection<Committee> Committees { get; set; } = new List<Committee>();
}
