using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class MeetingUserVote
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int VottingOptionId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int MeetingAgendaId { get; set; }

    public virtual MeetingAgendum MeetingAgenda { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual VotingOption VottingOption { get; set; } = null!;
}
