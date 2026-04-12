using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class MeetingType
{
    public int Id { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
}
