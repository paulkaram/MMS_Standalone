using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class Dictionary
{
    public int Id { get; set; }

    public string Keyword { get; set; } = null!;

    public string Ar { get; set; } = null!;

    public string En { get; set; } = null!;
}
