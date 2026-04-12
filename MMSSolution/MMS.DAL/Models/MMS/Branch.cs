using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class Branch
{
    public int Id { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public virtual ICollection<Structure> Structures { get; set; } = new List<Structure>();
}
