using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class AppSetting
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Value { get; set; }

    public string? Category { get; set; }

    public string? Description { get; set; }
}
