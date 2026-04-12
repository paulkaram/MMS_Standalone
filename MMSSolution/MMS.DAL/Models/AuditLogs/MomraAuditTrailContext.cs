using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MMS.DAL.Models.AuditLogs;

public partial class MomraAuditTrailContext : DbContext
{
    public MomraAuditTrailContext(DbContextOptions<MomraAuditTrailContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<DatabaseName> DatabaseNames { get; set; }

    public virtual DbSet<Operation> Operations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_lActivityLog");

            entity.ToTable("ActivityLog");

            entity.Property(e => e.ActionName).HasMaxLength(100);
            entity.Property(e => e.AdditionalInfo).HasMaxLength(1000);
            entity.Property(e => e.ControllerName).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Username).HasMaxLength(50);

            // DCC Compliance (NCA DCC-1:2022 Section 2-4): Security audit fields
            entity.Property(e => e.IpAddress).HasMaxLength(45); // IPv6 max length
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.DeviceInfo).HasMaxLength(500);

            entity.HasOne(d => d.Operation).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.OperationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActivityLog_Operations");
        });

        modelBuilder.Entity<DatabaseName>(entity =>
        {
            entity.Property(e => e.Dbname)
                .HasMaxLength(256)
                .HasColumnName("DBName");
        });

        modelBuilder.Entity<Operation>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.OperationCodeName).HasMaxLength(256);
            entity.Property(e => e.OperationNameAr).HasMaxLength(256);
            entity.Property(e => e.OperationNameEn).HasMaxLength(256);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
