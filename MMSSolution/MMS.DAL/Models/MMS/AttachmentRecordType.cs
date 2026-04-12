using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class AttachmentRecordType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? DisplayNameAr { get; set; }

    public string? DisplayNameEn { get; set; }

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
