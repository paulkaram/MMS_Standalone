using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class Lookup
{
    public int Id { get; set; }

    public string? NameEn { get; set; }

    public string? NameAr { get; set; }

    public bool? ForSurvey { get; set; }

    public int? LookupIdRelatedTo { get; set; }

    public virtual ICollection<LookupItem> LookupItems { get; set; } = new List<LookupItem>();
}
