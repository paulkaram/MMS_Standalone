using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class User
{
    public string Id { get; set; } = null!;

    public string FullnameAr { get; set; } = null!;

    public string FullnameEn { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Mobile { get; set; }

    public string? NationalId { get; set; }

    public int DefaultLanguageId { get; set; }

    public bool Approved { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public bool? SmsEnabled { get; set; }

    public bool? EmailNotificationEnabled { get; set; }

    public bool HasProfilePicture { get; set; }

    public string? PasswordHash { get; set; }

    public virtual ICollection<AttachmentAnnotation> AttachmentAnnotations { get; set; } = new List<AttachmentAnnotation>();

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual ICollection<AttachmentsSignature> AttachmentsSignatures { get; set; } = new List<AttachmentsSignature>();

    public virtual ICollection<CommitteePermission> CommitteePermissions { get; set; } = new List<CommitteePermission>();


    public virtual Language DefaultLanguage { get; set; } = null!;

    public virtual ICollection<MeetingAgendum> MeetingAgenda { get; set; } = new List<MeetingAgendum>();

    public virtual ICollection<MeetingAgendaNote> MeetingAgendaNotes { get; set; } = new List<MeetingAgendaNote>();

    public virtual ICollection<MeetingAgendaRecommendation> MeetingAgendaRecommendationCreateByNavigations { get; set; } = new List<MeetingAgendaRecommendation>();

    public virtual ICollection<MeetingAgendaRecommendation> MeetingAgendaRecommendationOwnerNavigations { get; set; } = new List<MeetingAgendaRecommendation>();

    public virtual ICollection<MeetingAttendee> MeetingAttendees { get; set; } = new List<MeetingAttendee>();

    public virtual ICollection<MeetingSummary> MeetingSummaries { get; set; } = new List<MeetingSummary>();

    public virtual ICollection<MeetingUserVote> MeetingUserVotes { get; set; } = new List<MeetingUserVote>();

    public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();

    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();

    public virtual ICollection<PermissionMatrix> PermissionMatrices { get; set; } = new List<PermissionMatrix>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual ICollection<UserCommittee> UserCommittees { get; set; } = new List<UserCommittee>();

    public virtual ICollection<UserSignature> UserSignatures { get; set; } = new List<UserSignature>();

    public virtual ICollection<UserStructure> UserStructures { get; set; } = new List<UserStructure>();

    public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
