using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class MeetingAgendum
{
    public int Id { get; set; }

    public int MeetingId { get; set; }

    public string Title { get; set; } = null!;

    public int Duration { get; set; }

    public int VotingTypeId { get; set; }

    public int? CommitteeDutyId { get; set; }

    public string? Note { get; set; }

    public DateTime? ActualStartDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    public bool? Paused { get; set; }

    public int? PauseDuration { get; set; }

    public int? FinalVotingOptionId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? LastPausedDate { get; set; }

    public virtual ICollection<AgendaTopic> AgendaTopics { get; set; } = new List<AgendaTopic>();

    public virtual CommitteeDuty? CommitteeDuty { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual VotingOption? FinalVotingOption { get; set; }

    public virtual Meeting Meeting { get; set; } = null!;

    public virtual ICollection<MeetingAgendaNote> MeetingAgendaNotes { get; set; } = new List<MeetingAgendaNote>();

    public virtual ICollection<MeetingAgendaRecommendation> MeetingAgendaRecommendations { get; set; } = new List<MeetingAgendaRecommendation>();

    public virtual ICollection<MeetingUserVote> MeetingUserVotes { get; set; } = new List<MeetingUserVote>();

    public virtual VotingType VotingType { get; set; } = null!;

    public virtual ICollection<MeetingAgendaSummary> MeetingAgendaSummaries { get; set; } = new List<MeetingAgendaSummary>();
}
