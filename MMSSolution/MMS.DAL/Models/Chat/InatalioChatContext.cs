using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MMS.DAL.Models.Chat;

public partial class InatalioChatContext : DbContext
{
	public InatalioChatContext()
	{
	}
	public InatalioChatContext(DbContextOptions<InatalioChatContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<ChatMember> ChatMembers { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ChatMessageContent> ChatMessageContents { get; set; }

    public virtual DbSet<ChatMessageType> ChatMessageTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(1000);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<ChatMember>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_ChatMembers_UserId");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Nickname).HasMaxLength(200);

            entity.HasOne(d => d.Chat).WithMany(p => p.ChatMembers)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMembers_Chats");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasIndex(e => e.ChatId, "IX_ChatMessages_ChatId");

            entity.HasIndex(e => e.SentAt, "IX_ChatMessages_SentAt").IsDescending();

            entity.Property(e => e.MessageText).HasDefaultValueSql("('')");
            entity.Property(e => e.SentAt).HasColumnType("datetime");

            entity.HasOne(d => d.Chat).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMessages_Chats");
        });

        modelBuilder.Entity<ChatMessageContent>(entity =>
        {
            entity.HasIndex(e => e.ChatMessageId, "IX_ChatMessageContents_ChatMessageId");

            entity.Property(e => e.RelativePath).HasMaxLength(1000);

            entity.HasOne(d => d.ChatMessage).WithMany(p => p.ChatMessageContents)
                .HasForeignKey(d => d.ChatMessageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMessageContents_ChatMessages");

            entity.HasOne(d => d.ChatMessageType).WithMany(p => p.ChatMessageContents)
                .HasForeignKey(d => d.ChatMessageTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMessageContents_ChatMessageTypes");
        });

        modelBuilder.Entity<ChatMessageType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
