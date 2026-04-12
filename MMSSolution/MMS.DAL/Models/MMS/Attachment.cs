using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class Attachment
{
    public int Id { get; set; }

    public int Version { get; set; }

    public int RecordId { get; set; }

    public int RecordTypeId { get; set; }

    public string? Title { get; set; }

    public string? FileName { get; set; }

    public string FileRelativeUrl { get; set; } = null!;

    public int? FileSize { get; set; }

    public bool Deleted { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public short PrivacyId { get; set; }

    public virtual ICollection<AttachmentAnnotation> AttachmentAnnotations { get; set; } = new List<AttachmentAnnotation>();

    public virtual ICollection<AttachmentVersion> AttachmentVersions { get; set; } = new List<AttachmentVersion>();

    public virtual ICollection<AttachmentsSignature> AttachmentsSignatures { get; set; } = new List<AttachmentsSignature>();

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Privacy Privacy { get; set; } = null!;

    public virtual AttachmentRecordType RecordType { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
