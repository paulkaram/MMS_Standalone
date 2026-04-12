using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class MeetingSummary
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
}
