using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class StructureType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Structure> Structures { get; set; } = new List<Structure>();
}
