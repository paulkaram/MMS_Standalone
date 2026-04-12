using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class LookupItem
{
    public int RecordPk { get; set; }

    public int LookupId { get; set; }

    public int? Id { get; set; }

    public string? NameEn { get; set; }

    public string? NameAr { get; set; }

    public int? ItemFromRelatedLookup { get; set; }

    public virtual Lookup Lookup { get; set; } = null!;
}
