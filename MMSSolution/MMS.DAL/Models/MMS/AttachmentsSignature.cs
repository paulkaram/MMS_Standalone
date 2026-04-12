using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class AttachmentsSignature
{
    public int Id { get; set; }

    public int AttachmentId { get; set; }

    public string UserId { get; set; } = null!;

    public virtual Attachment Attachment { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
