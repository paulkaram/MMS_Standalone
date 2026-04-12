using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class ScheduleQueue
{
    public int Id { get; set; }

    public int? ScheduleId { get; set; }

    public DateTime? LastStartDate { get; set; }

    public DateTime? LastEndDate { get; set; }

    public DateTime? LastSuccessDate { get; set; }

    public bool? IsRunning { get; set; }

    public bool? IsFail { get; set; }

    public bool? IsActive { get; set; }

    public string? ErrorMessage { get; set; }

    public virtual Schedule? Schedule { get; set; }
}
