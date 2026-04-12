using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class VotingOption
{
    public int Id { get; set; }

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public decimal? Weight { get; set; }

    public bool? Active { get; set; }

    public int? DisplayOrder { get; set; }

    public int? VotingTypeId { get; set; }

    public virtual ICollection<MeetingAgendum> MeetingAgenda { get; set; } = new List<MeetingAgendum>();

    public virtual ICollection<MeetingUserVote> MeetingUserVotes { get; set; } = new List<MeetingUserVote>();

    public virtual VotingType? VotingType { get; set; }
}
