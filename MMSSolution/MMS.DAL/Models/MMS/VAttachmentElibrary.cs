using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class VAttachmentElibrary
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public int RecordTypeId { get; set; }
}
