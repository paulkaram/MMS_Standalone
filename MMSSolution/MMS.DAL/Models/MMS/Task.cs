using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class Task
{
    public int Id { get; set; }

    public int MeetingId { get; set; }

    public string UserId { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public int DueDate { get; set; }

    public int StatusId { get; set; }

    public int TypeId { get; set; }

    public bool? Claimed { get; set; }

    public DateTime? ClaimedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public int? AttachmentId { get; set; }

    public virtual Attachment? Attachment { get; set; }

    public virtual Meeting Meeting { get; set; } = null!;

    public virtual ICollection<MeetingNote> MeetingNotes { get; set; } = new List<MeetingNote>();

    public virtual TaskStatus Status { get; set; } = null!;

    public virtual TaskType Type { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
