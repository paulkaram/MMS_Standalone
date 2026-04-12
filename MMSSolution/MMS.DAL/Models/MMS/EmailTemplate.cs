using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class EmailTemplate
{
    public int Id { get; set; }

    public string AppCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? SendTo { get; set; }

    public string? Subject { get; set; }

    public string Body { get; set; } = null!;
}
