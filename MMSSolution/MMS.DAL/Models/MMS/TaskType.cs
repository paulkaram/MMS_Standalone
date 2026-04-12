using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class TaskType
{
    public int Id { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
