using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class Privacy
{
    public short Id { get; set; }

    public string Name { get; set; } = null!;

    public string? NameAr { get; set; }

    public short Level { get; set; }

    public bool IsDefault { get; set; }

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual ICollection<UserCommittee> UserCommittees { get; set; } = new List<UserCommittee>();
}
