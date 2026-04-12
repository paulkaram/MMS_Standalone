using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class AttachmentVersion
{
    public int Id { get; set; }

    public int AttachementId { get; set; }

    public int Version { get; set; }

    public string FileRelativeUrl { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string FileName { get; set; } = null!;

    public virtual Attachment Attachement { get; set; } = null!;
}
