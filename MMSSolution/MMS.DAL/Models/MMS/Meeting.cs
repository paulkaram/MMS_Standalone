using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class Meeting
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string ReferenceNumber { get; set; } = null!;

    public DateTime Date { get; set; }

    public string StartTime { get; set; } = null!;

    public string EndTime { get; set; } = null!;

    public int MeetingTypeId { get; set; }

    public int? StatusId { get; set; }

    public string? Location { get; set; }

    public string? MeetingUrl { get; set; }

    public bool? IsOnlineMeeting { get; set; }

    public string? OnlineMeetingId { get; set; }

    public string? OnlineMeetingPasscode { get; set; }

    public bool? IsCommittee { get; set; }

    public int? CommitteeId { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Createdby { get; set; }

    public int? MeetingSummaryId { get; set; }

    public bool? IsPublicVoting { get; set; }

    public int? CouncilSessionId { get; set; }

    public virtual Committee? Committee { get; set; }

    public virtual CouncilSession? CouncilSession { get; set; }

    public virtual User? CreatedbyNavigation { get; set; }

    public virtual ICollection<MeetingAgendum> MeetingAgenda { get; set; } = new List<MeetingAgendum>();

    public virtual ICollection<MeetingAttendee> MeetingAttendees { get; set; } = new List<MeetingAttendee>();

    public virtual ICollection<MeetingNote> MeetingNotes { get; set; } = new List<MeetingNote>();

    public virtual MeetingSummary? MeetingSummary { get; set; }

    public virtual MeetingType MeetingType { get; set; } = null!;

    public virtual MeetingStatus? Status { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual ICollection<Meeting> Associateds { get; set; } = new List<Meeting>();

    public virtual ICollection<Meeting> Ids { get; set; } = new List<Meeting>();
}
