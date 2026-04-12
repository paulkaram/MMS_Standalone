using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class Note
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public int ActivityInstanceId { get; set; }

    public int StructureId { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Structure Structure { get; set; } = null!;
}
