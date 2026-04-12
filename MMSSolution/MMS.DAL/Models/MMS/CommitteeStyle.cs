using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class CommitteeStyle
{
    public int Id { get; set; }

    public string? NameEn { get; set; }

    public string? NameAr { get; set; }

    public virtual ICollection<Committee> Committees { get; set; } = new List<Committee>();
}
