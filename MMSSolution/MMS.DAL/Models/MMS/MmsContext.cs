using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MMS.DAL.Models.MMS;

public partial class MmsContext : DbContext
{
    public MmsContext(DbContextOptions<MmsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccountType> AccountTypes { get; set; }

    public virtual DbSet<AgendaTopic> AgendaTopics { get; set; }

    public virtual DbSet<ViewerToken> ViewerTokens { get; set; }

    public virtual DbSet<AppSetting> AppSettings { get; set; }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<AttachmentAnnotation> AttachmentAnnotations { get; set; }

    public virtual DbSet<AttachmentRecordType> AttachmentRecordTypes { get; set; }

    public virtual DbSet<AttachmentVersion> AttachmentVersions { get; set; }

    public virtual DbSet<AttachmentsSignature> AttachmentsSignatures { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Committee> Committees { get; set; }

    public virtual DbSet<CommitteeActivity> CommitteeActivities { get; set; }

    public virtual DbSet<CommitteeClassification> CommitteeClassifications { get; set; }

    public virtual DbSet<CommitteeDuty> CommitteeDuties { get; set; }

    public virtual DbSet<CommitteePermission> CommitteePermissions { get; set; }

    public virtual DbSet<CommitteeRole> CommitteeRoles { get; set; }

    public virtual DbSet<CommitteeStatus> CommitteeStatuses { get; set; }

    public virtual DbSet<CommitteeStyle> CommitteeStyles { get; set; }

    public virtual DbSet<CommitteeType> CommitteeTypes { get; set; }

    public virtual DbSet<CouncilSession> CouncilSessions { get; set; }

    public virtual DbSet<DataSource> DataSources { get; set; }

    public virtual DbSet<Dictionary> Dictionaries { get; set; }

    public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Lookup> Lookups { get; set; }

    public virtual DbSet<LookupItem> LookupItems { get; set; }

    public virtual DbSet<Meeting> Meetings { get; set; }

    public virtual DbSet<MeetingAgendaNote> MeetingAgendaNotes { get; set; }

    public virtual DbSet<MeetingAgendaRecommendation> MeetingAgendaRecommendations { get; set; }

    public virtual DbSet<MeetingAgendaRecommendationStatus> MeetingAgendaRecommendationStatuses { get; set; }

    public virtual DbSet<Priority> Priorities { get; set; }

    public virtual DbSet<MeetingAgendum> MeetingAgenda { get; set; }

    public virtual DbSet<MeetingAttendee> MeetingAttendees { get; set; }

    public virtual DbSet<MeetingNote> MeetingNotes { get; set; }

    public virtual DbSet<MeetingStatus> MeetingStatuses { get; set; }

    public virtual DbSet<MeetingSummary> MeetingSummaries { get; set; }

    public virtual DbSet<MeetingTranscript> MeetingTranscripts { get; set; }

    public virtual DbSet<MeetingAgendaSummary> MeetingAgendaSummaries { get; set; }

    public virtual DbSet<MeetingType> MeetingTypes { get; set; }

    public virtual DbSet<MeetingUserVote> MeetingUserVotes { get; set; }

    public virtual DbSet<MomTemplate> MomTemplates { get; set; }

    public virtual DbSet<Note> Notes { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<PermissionLevel> PermissionLevels { get; set; }

    public virtual DbSet<PermissionMatrix> PermissionMatrices { get; set; }

    public virtual DbSet<PermissionType> PermissionTypes { get; set; }

    public virtual DbSet<Privacy> Privacies { get; set; }

    public virtual DbSet<RecommendationNote> RecommendationNotes { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleType> RoleTypes { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<ScheduleQueue> ScheduleQueues { get; set; }

    public virtual DbSet<SignatureType> SignatureTypes { get; set; }

    public virtual DbSet<Stamp> Stamps { get; set; }

    public virtual DbSet<Structure> Structures { get; set; }

    public virtual DbSet<StructureType> StructureTypes { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskStatus> TaskStatuses { get; set; }

    public virtual DbSet<TaskType> TaskTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserCommittee> UserCommittees { get; set; }

    public virtual DbSet<UserSignature> UserSignatures { get; set; }

    public virtual DbSet<UserStructure> UserStructures { get; set; }

    public virtual DbSet<VotingOption> VotingOptions { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<SessionItem> SessionItems { get; set; }

    public virtual DbSet<SessionItemType> SessionItemTypes { get; set; }

    public virtual DbSet<VotingType> VotingTypes { get; set; }

    public virtual DbSet<RoleMenuPermission> RoleMenuPermissions { get; set; }

    public virtual DbSet<GroupMenuPermission> GroupMenuPermissions { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<CommitteeItem> CommitteeItems { get; set; }

    public virtual DbSet<CommitteeItemType> CommitteeItemTypes { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<TagLink> TagLinks { get; set; }

    public virtual DbSet<ExternalMember> ExternalMembers { get; set; }

    public virtual DbSet<CommitteeExternalMember> CommitteeExternalMembers { get; set; }

    public virtual DbSet<Delegation> Delegations { get; set; }

    public virtual DbSet<DelegationTask> DelegationTasks { get; set; }

    public virtual DbSet<Bid> Bids { get; set; }

    public virtual DbSet<BidStatus> BidStatuses { get; set; }

    public virtual DbSet<BidStakeholder> BidStakeholders { get; set; }

    public virtual DbSet<BidStatusHistory> BidStatusHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountType>(entity =>
        {
            entity.ToTable("AccountType");

            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<AgendaTopic>(entity =>
        {
            entity.ToTable("AgendaTopic");

            entity.Property(e => e.Text).HasMaxLength(4000);

            entity.HasOne(d => d.MeetingAgenda).WithMany(p => p.AgendaTopics)
                .HasForeignKey(d => d.MeetingAgendaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AgendaTopic_MeetingAgenda");
        });

        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.Property(e => e.Category).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Value).HasMaxLength(500);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.ToTable("Attachment");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.FileName).HasMaxLength(500);
            entity.Property(e => e.FileRelativeUrl).HasMaxLength(4000);
            entity.Property(e => e.PrivacyId).HasDefaultValue((short)1);
            entity.Property(e => e.Title).HasMaxLength(500);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attachment_User");

            entity.HasOne(d => d.Privacy).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.PrivacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attachment_Privacy");

            entity.HasOne(d => d.RecordType).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.RecordTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attachment_AttachmentRecordType");
        });

        modelBuilder.Entity<AttachmentAnnotation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Attachment_Annotaion");

            entity.ToTable("Attachment_Annotation");

            entity.HasOne(d => d.Attachment).WithMany(p => p.AttachmentAnnotations)
                .HasForeignKey(d => d.AttachmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attachment_Annotaion_Attachment");

            entity.HasOne(d => d.User).WithMany(p => p.AttachmentAnnotations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attachment_Annotaion_User");
        });

        modelBuilder.Entity<AttachmentRecordType>(entity =>
        {
            entity.ToTable("AttachmentRecordType");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DisplayNameAr).HasMaxLength(500);
            entity.Property(e => e.DisplayNameEn).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<AttachmentVersion>(entity =>
        {
            entity.ToTable("AttachmentVersion");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.FileName).HasMaxLength(500);
            entity.Property(e => e.FileRelativeUrl).HasMaxLength(4000);

            entity.HasOne(d => d.Attachement).WithMany(p => p.AttachmentVersions)
                .HasForeignKey(d => d.AttachementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttachmentVersion_Attachment");
        });

        modelBuilder.Entity<AttachmentsSignature>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");

            entity.HasOne(d => d.Attachment).WithMany(p => p.AttachmentsSignatures)
                .HasForeignKey(d => d.AttachmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttachmentsSignatures_Attachment");

            entity.HasOne(d => d.User).WithMany(p => p.AttachmentsSignatures)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttachmentsSignatures_User");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branch");

            entity.Property(e => e.NameAr).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);
        });

        modelBuilder.Entity<Committee>(entity =>
        {
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.HasAdditionalMembers).HasColumnName("hasAdditionalMembers");
            entity.Property(e => e.HasFinancialCompensation).HasColumnName("hasFinancialCompensation");
            entity.Property(e => e.NameAr).HasMaxLength(1000);
            entity.Property(e => e.NameEn).HasMaxLength(1000);
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.CommitteeClassification).WithMany(p => p.Committees)
                .HasForeignKey(d => d.CommitteeClassificationId)
                .HasConstraintName("FK_Committees_CommitteeClassification");

            entity.HasOne(d => d.CommitteeStatus).WithMany(p => p.Committees)
                .HasForeignKey(d => d.CommitteeStatusId)
                .HasConstraintName("FK_Committees_CommitteeStatus");

            entity.HasOne(d => d.CommitteeStyle).WithMany(p => p.Committees)
                .HasForeignKey(d => d.CommitteeStyleId)
                .HasConstraintName("FK_Committees_CommitteeStyle");

            entity.HasOne(d => d.Type).WithMany(p => p.Committees)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Committees_CommitteeType");
        });

        modelBuilder.Entity<CommitteeActivity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Committee_Members");

            entity.HasOne(d => d.Committee).WithMany(p => p.CommitteeActivities)
                .HasForeignKey(d => d.CommitteeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommitteeActivities_Committees");
        });

        modelBuilder.Entity<CommitteeClassification>(entity =>
        {
            entity.ToTable("CommitteeClassification");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.NameAr).HasMaxLength(500);
            entity.Property(e => e.NameEn).HasMaxLength(500);
        });

        modelBuilder.Entity<CommitteeDuty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_CommitteeMembers");

            entity.HasOne(d => d.Committee).WithMany(p => p.CommitteeDuties)
                .HasForeignKey(d => d.CommitteeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommitteeDuties_Committees");
        });

        modelBuilder.Entity<CommitteePermission>(entity =>
        {
            entity.HasOne(d => d.Committee).WithMany(p => p.CommitteePermissions)
                .HasForeignKey(d => d.CommitteeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommitteePermission_Committee");

            entity.HasOne(d => d.Permission).WithMany(p => p.CommitteePermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommitteePermission_Permission");

            entity.HasOne(d => d.User).WithMany(p => p.CommitteePermissions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommitteePermission_User");
        });

        modelBuilder.Entity<CommitteeRole>(entity =>
        {
            entity.ToTable("CommitteeRole");

            entity.Property(e => e.NameAr).HasMaxLength(1000);
            entity.Property(e => e.NameEn).HasMaxLength(1000);
        });

        modelBuilder.Entity<CommitteeStatus>(entity =>
        {
            entity.ToTable("CommitteeStatus");

            entity.Property(e => e.NameAr)
                .HasMaxLength(50)
                .HasColumnName("Name_Ar");
            entity.Property(e => e.NameEn)
                .HasMaxLength(50)
                .HasColumnName("Name_En");
        });

        modelBuilder.Entity<CommitteeStyle>(entity =>
        {
            entity.ToTable("CommitteeStyle");

            entity.Property(e => e.NameAr).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);
        });

        modelBuilder.Entity<CommitteeType>(entity =>
        {
            entity.ToTable("CommitteeType");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.NameAr).HasMaxLength(500);
            entity.Property(e => e.NameEn).HasMaxLength(500);
        });

        modelBuilder.Entity<CouncilSession>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.NameAr).HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
        });

        modelBuilder.Entity<DataSource>(entity =>
        {
            entity.Property(e => e.Dbname)
                .HasMaxLength(100)
                .HasColumnName("DBName");
            entity.Property(e => e.InstanceName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<Dictionary>(entity =>
        {
            entity.ToTable("Dictionary");

            entity.Property(e => e.Ar)
                .HasMaxLength(200)
                .HasColumnName("AR");
            entity.Property(e => e.En)
                .HasMaxLength(200)
                .HasColumnName("EN");
            entity.Property(e => e.Keyword).HasMaxLength(100);
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.ToTable("EmailTemplate");

            entity.Property(e => e.AppCode).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.SendTo).HasMaxLength(200);
            entity.Property(e => e.Subject).HasMaxLength(200);
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.ToTable("Language");

            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(20);
        });

        modelBuilder.Entity<Lookup>(entity =>
        {
            entity.ToTable("Lookup");

            entity.Property(e => e.NameAr).HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
        });

        modelBuilder.Entity<LookupItem>(entity =>
        {
            entity.HasKey(e => e.RecordPk);

            entity.Property(e => e.RecordPk).HasColumnName("Record_PK");
            entity.Property(e => e.NameAr).HasMaxLength(250);
            entity.Property(e => e.NameEn)
                .HasMaxLength(250)
                .HasColumnName("NameEN");

            entity.HasOne(d => d.Lookup).WithMany(p => p.LookupItems)
                .HasForeignKey(d => d.LookupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lookup_LookupItems");
        });

        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToTable("Meeting");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(1000);
            entity.Property(e => e.MeetingUrl).HasMaxLength(4000);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(50);
            entity.Property(e => e.StartTime).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(1000);

            entity.HasOne(d => d.Committee).WithMany(p => p.Meetings)
                .HasForeignKey(d => d.CommitteeId)
                .HasConstraintName("FK_Meeting_Committees");

            entity.HasOne(d => d.CouncilSession).WithMany(p => p.Meetings)
                .HasForeignKey(d => d.CouncilSessionId)
                .HasConstraintName("FK_Meeting_CouncilSessions");

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.Meetings)
                .HasForeignKey(d => d.Createdby)
                .HasConstraintName("FK_Meeting_User");

            entity.HasOne(d => d.MeetingSummary).WithMany(p => p.Meetings)
                .HasForeignKey(d => d.MeetingSummaryId)
                .HasConstraintName("FK_Meeting_MeetingSummary");

            entity.HasOne(d => d.MeetingType).WithMany(p => p.Meetings)
                .HasForeignKey(d => d.MeetingTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Meeting_MeetingType");

            entity.HasOne(d => d.Status).WithMany(p => p.Meetings)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK_Meeting_MeetingStatus");

            entity.HasMany(d => d.Associateds).WithMany(p => p.Ids)
                .UsingEntity<Dictionary<string, object>>(
                    "AssociatedMeeting",
                    r => r.HasOne<Meeting>().WithMany()
                        .HasForeignKey("AssociatedId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AssociatedMeetings_Meeting1"),
                    l => l.HasOne<Meeting>().WithMany()
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AssociatedMeetings_Meeting"),
                    j =>
                    {
                        j.HasKey("Id", "AssociatedId");
                        j.ToTable("AssociatedMeetings");
                    });

            entity.HasMany(d => d.Ids).WithMany(p => p.Associateds)
                .UsingEntity<Dictionary<string, object>>(
                    "AssociatedMeeting",
                    r => r.HasOne<Meeting>().WithMany()
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AssociatedMeetings_Meeting"),
                    l => l.HasOne<Meeting>().WithMany()
                        .HasForeignKey("AssociatedId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AssociatedMeetings_Meeting1"),
                    j =>
                    {
                        j.HasKey("Id", "AssociatedId");
                        j.ToTable("AssociatedMeetings");
                    });
        });

        modelBuilder.Entity<MeetingAgendaNote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_MeetingAgendaId");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsPublic).HasDefaultValue(true);

            entity.HasOne(d => d.MeetingAgenda).WithMany(p => p.MeetingAgendaNotes)
                .HasForeignKey(d => d.MeetingAgendaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingAgendaNotes_MeetingAgenda");

            entity.HasOne(d => d.User).WithMany(p => p.MeetingAgendaNotes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingAgendaNotes_User");
        });

        modelBuilder.Entity<MeetingAgendaRecommendation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_AgendaRecommendation");

            entity.ToTable("MeetingAgendaRecommendation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");

            entity.HasOne(d => d.CreateByNavigation).WithMany(p => p.MeetingAgendaRecommendationCreateByNavigations)
                .HasForeignKey(d => d.CreateBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingAgendaRecommendation_User");

            entity.HasOne(d => d.MeetingAgenda).WithMany(p => p.MeetingAgendaRecommendations)
                .HasForeignKey(d => d.MeetingAgendaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingAgendaRecommendation_MeetingAgenda");

            entity.HasOne(d => d.OwnerNavigation).WithMany(p => p.MeetingAgendaRecommendationOwnerNavigations)
                .HasForeignKey(d => d.Owner)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingAgendaRecommendation_User1");

            entity.HasOne(d => d.Status).WithMany(p => p.MeetingAgendaRecommendations)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingAgendaRecommendation_MeetingAgendaRecommendation");
        });

        modelBuilder.Entity<MeetingAgendaRecommendationStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_AgendaRecommendationType");

            entity.ToTable("MeetingAgendaRecommendationStatus");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(1000);
            entity.Property(e => e.NameAr).HasMaxLength(1000);
            entity.Property(e => e.NameEn).HasMaxLength(1000);
        });

        modelBuilder.Entity<Priority>(entity =>
        {
            entity.ToTable("Priority");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.NameAr).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);
        });

        modelBuilder.Entity<MeetingAgendum>(entity =>
        {
            entity.Property(e => e.ActualEndDate).HasColumnType("datetime");
            entity.Property(e => e.ActualStartDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.LastPausedDate).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(4000);
            entity.Property(e => e.Title).HasMaxLength(4000);

            entity.HasOne(d => d.CommitteeDuty).WithMany(p => p.MeetingAgenda)
                .HasForeignKey(d => d.CommitteeDutyId)
                .HasConstraintName("FK_MeetingAgenda_CommitteeDuties");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.MeetingAgenda)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingAgenda_User");

            entity.HasOne(d => d.FinalVotingOption).WithMany(p => p.MeetingAgenda)
                .HasForeignKey(d => d.FinalVotingOptionId)
                .HasConstraintName("FK_MeetingAgenda_VotingOptions");

            entity.HasOne(d => d.Meeting).WithMany(p => p.MeetingAgenda)
                .HasForeignKey(d => d.MeetingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingAgenda_Meeting");

            entity.HasOne(d => d.VotingType).WithMany(p => p.MeetingAgenda)
                .HasForeignKey(d => d.VotingTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingAgenda_VotingType");
        });

        modelBuilder.Entity<MeetingAttendee>(entity =>
        {
            entity.Property(e => e.JobTitle).HasMaxLength(4000);

            entity.HasOne(d => d.Meeting).WithMany(p => p.MeetingAttendees)
                .HasForeignKey(d => d.MeetingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingAttendees_Meeting");

            entity.HasOne(d => d.User).WithMany(p => p.MeetingAttendees)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_MeetingAttendees_User");

            entity.HasOne(d => d.ExternalMember).WithMany(p => p.MeetingAttendees)
                .HasForeignKey(d => d.ExternalMemberId)
                .HasConstraintName("FK_MeetingAttendees_External");
        });

        modelBuilder.Entity<MeetingNote>(entity =>
        {
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Meeting).WithMany(p => p.MeetingNotes)
                .HasForeignKey(d => d.MeetingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingNotes_Meeting");

            entity.HasOne(d => d.Task).WithMany(p => p.MeetingNotes)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("FK_MeetingNotes_Tasks");
        });

        modelBuilder.Entity<MeetingStatus>(entity =>
        {
            entity.ToTable("MeetingStatus");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.NameAr).HasMaxLength(1000);
            entity.Property(e => e.NameEn).HasMaxLength(1000);
        });

        modelBuilder.Entity<MeetingSummary>(entity =>
        {
            entity.ToTable("MeetingSummary");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.MeetingSummaries)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingSummary_User");
        });

        modelBuilder.Entity<MeetingAgendaSummary>(entity =>
        {
            entity.ToTable("MeetingAgendaSummary");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MeetingAgenda).WithMany(p => p.MeetingAgendaSummaries)
                .HasForeignKey(d => d.MeetingAgendaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingAgendaSummary_MeetingAgenda");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingAgendaSummary_User");
        });

        modelBuilder.Entity<MeetingType>(entity =>
        {
            entity.ToTable("MeetingType");
        });

        modelBuilder.Entity<MomTemplate>(entity =>
        {
            entity.ToTable("MomTemplate");

            entity.Property(e => e.NameAr).HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);

            entity.HasOne(d => d.Branch).WithMany()
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_MomTemplate_Branch");
        });

        modelBuilder.Entity<MeetingUserVote>(entity =>
        {
            entity.ToTable("MeetingUserVote");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.MeetingAgenda).WithMany(p => p.MeetingUserVotes)
                .HasForeignKey(d => d.MeetingAgendaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingUserVote_MeetingAgenda");

            entity.HasOne(d => d.User).WithMany(p => p.MeetingUserVotes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingUserVote_User");

            entity.HasOne(d => d.VottingOption).WithMany(p => p.MeetingUserVotes)
                .HasForeignKey(d => d.VottingOptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeetingUserVote_VotingOptions");
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.ToTable("Note");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Text).HasMaxLength(4000);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Notes)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Note_User");

            entity.HasOne(d => d.Structure).WithMany(p => p.Notes)
                .HasForeignKey(d => d.StructureId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Note_Structure");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permission");

            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.GroupName).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Type).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Permission_PermissionType");
        });

        modelBuilder.Entity<PermissionLevel>(entity =>
        {
            entity.ToTable("PermissionLevel");

            entity.Property(e => e.Name).HasMaxLength(10);
        });

        modelBuilder.Entity<PermissionMatrix>(entity =>
        {
            entity.ToTable("PermissionMatrix");

            entity.HasOne(d => d.Level).WithMany(p => p.PermissionMatrices)
                .HasForeignKey(d => d.LevelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PermissionMatrix_PermissionLevel");

            entity.HasOne(d => d.Permission).WithMany(p => p.PermissionMatrices)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PermissionMatrix_Permission");

            entity.HasOne(d => d.Role).WithMany(p => p.PermissionMatrices)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_PermissionMatrix_Role");

            entity.HasOne(d => d.Structure).WithMany(p => p.PermissionMatrices)
                .HasForeignKey(d => d.StructureId)
                .HasConstraintName("FK_PermissionMatrix_Structure");

            entity.HasOne(d => d.User).WithMany(p => p.PermissionMatrices)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_PermissionMatrix_User");
        });

        modelBuilder.Entity<PermissionType>(entity =>
        {
            entity.ToTable("PermissionType");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Privacy>(entity =>
        {
            entity.ToTable("Privacy");

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.NameAr).HasMaxLength(50);
        });

        modelBuilder.Entity<RecommendationNote>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC070C1D7878");

            entity.HasIndex(e => e.Token, "UQ__RefreshT__1EB4F8171AA9C52C").IsUnique();

            entity.Property(e => e.Expiration).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.RoleNameAr).HasMaxLength(200);
            entity.Property(e => e.RoleNameEn).HasMaxLength(200);

            entity.HasOne(d => d.Type).WithMany(p => p.Roles)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Role_RoleType");
        });

        modelBuilder.Entity<RoleType>(entity =>
        {
            entity.ToTable("RoleType");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.ToTable("Schedule");

            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<ScheduleQueue>(entity =>
        {
            entity.ToTable("ScheduleQueue");

            entity.Property(e => e.LastEndDate).HasColumnType("datetime");
            entity.Property(e => e.LastStartDate).HasColumnType("datetime");
            entity.Property(e => e.LastSuccessDate).HasColumnType("datetime");

            entity.HasOne(d => d.Schedule).WithMany(p => p.ScheduleQueues)
                .HasForeignKey(d => d.ScheduleId)
                .HasConstraintName("FK_ScheduleQueue_Schedule");
        });

        modelBuilder.Entity<SignatureType>(entity =>
        {
            entity.ToTable("SignatureType");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Structure>(entity =>
        {
            entity.ToTable("Structure");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.NameAr).HasMaxLength(500);
            entity.Property(e => e.NameEn).HasMaxLength(500);

            entity.HasOne(d => d.Branch).WithMany(p => p.Structures)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK_Structure_Branch");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_Structure_Structure");

            entity.HasOne(d => d.Type).WithMany(p => p.Structures)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Structure_StructureType");
        });

        modelBuilder.Entity<StructureType>(entity =>
        {
            entity.ToTable("StructureType");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.Property(e => e.ClaimedDate).HasColumnType("datetime");
            entity.Property(e => e.CompletedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Attachment).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.AttachmentId)
                .HasConstraintName("FK_Tasks_Attachment");

            entity.HasOne(d => d.Meeting).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.MeetingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tasks_Meeting");

            entity.HasOne(d => d.Status).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tasks_TaskStatus");

            entity.HasOne(d => d.Type).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tasks_TaskType");

            entity.HasOne(d => d.User).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tasks_User");
        });

        modelBuilder.Entity<TaskStatus>(entity =>
        {
            entity.ToTable("TaskStatus");

            entity.Property(e => e.NameAr).HasMaxLength(1000);
            entity.Property(e => e.NameEn).HasMaxLength(1000);
        });

        modelBuilder.Entity<TaskType>(entity =>
        {
            entity.ToTable("TaskType");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.NameAr).HasMaxLength(1000);
            entity.Property(e => e.NameEn).HasMaxLength(1000);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(500);
            entity.Property(e => e.FullnameAr).HasMaxLength(200);
            entity.Property(e => e.FullnameEn).HasMaxLength(200);
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.Mobile).HasMaxLength(50);
            entity.Property(e => e.NationalId).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);

            entity.HasOne(d => d.DefaultLanguage).WithMany(p => p.Users)
                .HasForeignKey(d => d.DefaultLanguageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Language");
        });

        modelBuilder.Entity<UserCommittee>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.CommitteeId, e.CommitteeRoleId });

            entity.ToTable("UserCommittee");

            entity.Property(e => e.Active).HasDefaultValue(true);

            entity.HasOne(d => d.Committee).WithMany(p => p.UserCommittees)
                .HasForeignKey(d => d.CommitteeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserCommittee_Committees");

            entity.HasOne(d => d.CommitteeRole).WithMany(p => p.UserCommittees)
                .HasForeignKey(d => d.CommitteeRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserCommittee_CommitteeRole");

            entity.HasOne(d => d.Privacy).WithMany(p => p.UserCommittees)
                .HasForeignKey(d => d.PrivacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserCommittee_Privacy");

            entity.HasOne(d => d.User).WithMany(p => p.UserCommittees)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserCommittee_User");
        });

        modelBuilder.Entity<UserSignature>(entity =>
        {
            entity.ToTable("UserSignature");

            entity.Property(e => e.LastAttempt).HasColumnType("datetime");
            entity.Property(e => e.LastSuccessfulAttempt).HasColumnType("datetime");

            entity.HasOne(d => d.Type).WithMany(p => p.UserSignatures)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserSignature_SignatureType");

            entity.HasOne(d => d.User).WithMany(p => p.UserSignatures)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserSignature_User");
        });

        modelBuilder.Entity<UserStructure>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.StrucutreId, e.RoleId });

            entity.ToTable("UserStructure");

            entity.HasOne(d => d.Role).WithMany(p => p.UserStructures)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserStructure_Role");

            entity.HasOne(d => d.Strucutre).WithMany(p => p.UserStructures)
                .HasForeignKey(d => d.StrucutreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserStructure_Structure");

            entity.HasOne(d => d.User).WithMany(p => p.UserStructures)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserStructure_User");
        });

        modelBuilder.Entity<VotingOption>(entity =>
        {
            entity.Property(e => e.NameAr).HasMaxLength(1000);
            entity.Property(e => e.NameEn).HasMaxLength(1000);
            entity.Property(e => e.Weight).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.VotingType).WithMany(p => p.VotingOptions)
                .HasForeignKey(d => d.VotingTypeId)
                .HasConstraintName("FK_VotingOptions_VotingType");
        });

        modelBuilder.Entity<VotingType>(entity =>
        {
            entity.ToTable("VotingType");

            entity.Property(e => e.NameAr).HasMaxLength(1000);
            entity.Property(e => e.NameEn).HasMaxLength(1000);
        });

        modelBuilder.Entity<SessionItemType>(entity =>
        {
            entity.Property(e => e.NameAr).HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.Property(e => e.ReferenceNumber).HasMaxLength(50);
            entity.Property(e => e.ExternalReferenceNumber).HasMaxLength(200);
            entity.Property(e => e.Subject).HasMaxLength(500);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.MeetingDate).HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Committee).WithMany()
                .HasForeignKey(d => d.CommitteeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sessions_Committees");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sessions_Users");
        });

        modelBuilder.Entity<SessionItem>(entity =>
        {
            entity.Property(e => e.ExternalId).HasMaxLength(200);
            entity.Property(e => e.Subject).HasMaxLength(500);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.Order).HasColumnName("Order");

            entity.HasOne(d => d.Session).WithMany(p => p.SessionItems)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SessionItems_Sessions");

            entity.HasOne(d => d.ItemType).WithMany(p => p.SessionItems)
                .HasForeignKey(d => d.ItemTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SessionItems_SessionItemTypes");

            entity.HasOne(d => d.RelatedSessionItem).WithMany(p => p.InverseRelatedSessionItem)
                .HasForeignKey(d => d.RelatedSessionItemId)
                .HasConstraintName("FK_SessionItems_RelatedItem");
        });

        modelBuilder.Entity<RoleMenuPermission>(entity =>
        {
            entity.ToTable("RoleMenuPermission");
            entity.HasOne(e => e.Permission).WithMany().HasForeignKey(e => e.PermissionId);
        });

        modelBuilder.Entity<GroupMenuPermission>(entity =>
        {
            entity.ToTable("GroupMenuPermission");
            entity.HasOne(e => e.Permission).WithMany().HasForeignKey(e => e.PermissionId);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.ToTable("Groups");
            entity.Property(e => e.NameAr).HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime").HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.GroupId });
            entity.ToTable("UserGroup");

            entity.HasOne(d => d.User).WithMany(p => p.UserGroups)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserGroup_User");

            entity.HasOne(d => d.Group).WithMany(p => p.UserGroups)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserGroup_Groups");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.ToTable("UserRole");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRole_User");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRole_Role");
        });

        modelBuilder.Entity<CommitteeItemType>(entity =>
        {
            entity.ToTable("CommitteeItemType");
            entity.Property(e => e.NameAr).HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
        });

        modelBuilder.Entity<ExternalMember>(entity =>
        {
            entity.ToTable("ExternalMember");
            entity.Property(e => e.FullnameAr).HasMaxLength(200);
            entity.Property(e => e.FullnameEn).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(500);
            entity.Property(e => e.Mobile).HasMaxLength(50);
            entity.Property(e => e.Organization).HasMaxLength(300);
            entity.Property(e => e.Position).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<CommitteeExternalMember>(entity =>
        {
            entity.ToTable("CommitteeExternalMember");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Committee).WithMany()
                .HasForeignKey(d => d.CommitteeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommitteeExternalMember_Committee");

            entity.HasOne(d => d.ExternalMember).WithMany(p => p.CommitteeMemberships)
                .HasForeignKey(d => d.ExternalMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommitteeExternalMember_External");

            entity.HasOne(d => d.CommitteeRole).WithMany()
                .HasForeignKey(d => d.CommitteeRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommitteeExternalMember_Role");

            entity.HasIndex(e => new { e.CommitteeId, e.ExternalMemberId, e.CommitteeRoleId }).IsUnique();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tag");
            entity.Property(e => e.NameAr).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.HasIndex(e => e.NameEn).IsUnique();
        });

        modelBuilder.Entity<TagLink>(entity =>
        {
            entity.ToTable("TagLink");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Tag).WithMany(p => p.TagLinks)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TagLink_Tag");

            entity.HasIndex(e => new { e.TagId, e.EntityTypeId, e.EntityId }).IsUnique();
            entity.HasIndex(e => new { e.EntityTypeId, e.EntityId });
        });

        modelBuilder.Entity<CommitteeItem>(entity =>
        {
            entity.ToTable("CommitteeItem");
            entity.Property(e => e.ReferenceNumber).HasMaxLength(50);
            entity.Property(e => e.ExternalReferenceNumber).HasMaxLength(200);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.Order).HasColumnName("Order");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Committee).WithMany()
                .HasForeignKey(d => d.CommitteeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommitteeItem_Committee");

            entity.HasOne(d => d.ItemType).WithMany(p => p.CommitteeItems)
                .HasForeignKey(d => d.ItemTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommitteeItem_ItemType");

            entity.HasOne(d => d.RelatedItem).WithMany(p => p.InverseRelatedItem)
                .HasForeignKey(d => d.RelatedItemId)
                .HasConstraintName("FK_CommitteeItem_RelatedItem");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommitteeItem_CreatedBy");

            entity.HasOne(d => d.Bid).WithMany(p => p.Items)
                .HasForeignKey(d => d.BidId)
                .HasConstraintName("FK_CommitteeItem_Bid");
        });

        modelBuilder.Entity<ViewerToken>(entity =>
        {
            entity.ToTable("ViewerToken");
            entity.Property(e => e.Token).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<Delegation>(entity =>
        {
            entity.ToTable("Delegation");
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.FromUser).WithMany()
                .HasForeignKey(d => d.FromUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Delegation_FromUser");

            entity.HasOne(d => d.ToUser).WithMany()
                .HasForeignKey(d => d.ToUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Delegation_ToUser");

            entity.HasIndex(e => new { e.FromUserId, e.IsActive });
            entity.HasIndex(e => new { e.ToUserId, e.IsActive });
        });

        modelBuilder.Entity<DelegationTask>(entity =>
        {
            entity.ToTable("DelegationTask");

            entity.HasOne(d => d.Delegation).WithMany(p => p.DelegationTasks)
                .HasForeignKey(d => d.DelegationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DelegationTask_Delegation");

            entity.HasOne(d => d.TaskNavigation).WithMany()
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DelegationTask_Task");

            entity.HasIndex(e => new { e.DelegationId, e.TaskId }).IsUnique();
        });

        modelBuilder.Entity<BidStatus>(entity =>
        {
            entity.ToTable("BidStatus");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.NameAr).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);
        });

        modelBuilder.Entity<Bid>(entity =>
        {
            entity.ToTable("Bid");
            entity.Property(e => e.ReferenceNumber).HasMaxLength(50);
            entity.Property(e => e.ExternalMeetingNumber).HasMaxLength(200);
            entity.Property(e => e.Subject).HasMaxLength(500);
            entity.Property(e => e.InitialMinutesPath).HasMaxLength(4000);
            entity.Property(e => e.FinalMinutesPath).HasMaxLength(4000);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StatusId).HasDefaultValue(1);

            entity.HasOne(d => d.Committee).WithMany()
                .HasForeignKey(d => d.CommitteeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bid_Committee");

            entity.HasOne(d => d.Status).WithMany(p => p.Bids)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bid_Status");

            entity.HasOne(d => d.TeamLeader).WithMany()
                .HasForeignKey(d => d.TeamLeaderUserId)
                .HasConstraintName("FK_Bid_TeamLeader");

            entity.HasOne(d => d.Meeting).WithMany()
                .HasForeignKey(d => d.MeetingId)
                .HasConstraintName("FK_Bid_Meeting");

            entity.HasOne(d => d.CreatedByNavigation).WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bid_CreatedBy");
        });

        modelBuilder.Entity<BidStakeholder>(entity =>
        {
            entity.ToTable("BidStakeholder");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Bid).WithMany(p => p.Stakeholders)
                .HasForeignKey(d => d.BidId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_BidStakeholder_Bid");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_BidStakeholder_User");

            entity.HasOne(d => d.ExternalMember).WithMany()
                .HasForeignKey(d => d.ExternalMemberId)
                .HasConstraintName("FK_BidStakeholder_External");
        });

        modelBuilder.Entity<BidStatusHistory>(entity =>
        {
            entity.ToTable("BidStatusHistory");
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.ChangedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Bid).WithMany(p => p.StatusHistory)
                .HasForeignKey(d => d.BidId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_BidStatusHistory_Bid");

            entity.HasOne(d => d.FromStatus).WithMany()
                .HasForeignKey(d => d.FromStatusId)
                .HasConstraintName("FK_BidStatusHistory_FromStatus");

            entity.HasOne(d => d.ToStatus).WithMany()
                .HasForeignKey(d => d.ToStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BidStatusHistory_ToStatus");

            entity.HasOne(d => d.ChangedByNavigation).WithMany()
                .HasForeignKey(d => d.ChangedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BidStatusHistory_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
