using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class Schedule
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool Active { get; set; }

    public virtual ICollection<ScheduleQueue> ScheduleQueues { get; set; } = new List<ScheduleQueue>();
}
