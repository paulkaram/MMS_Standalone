using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class VotingType
{
    public int Id { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public bool? Active { get; set; }

    public int? DisplayOrder { get; set; }

    public virtual ICollection<MeetingAgendum> MeetingAgenda { get; set; } = new List<MeetingAgendum>();

    public virtual ICollection<VotingOption> VotingOptions { get; set; } = new List<VotingOption>();
}
