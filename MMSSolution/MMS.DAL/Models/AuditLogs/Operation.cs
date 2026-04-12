using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.AuditLogs;

public partial class Operation
{
    public int Id { get; set; }

    public string OperationCodeName { get; set; } = null!;

    public string OperationNameEn { get; set; } = null!;

    public string OperationNameAr { get; set; } = null!;

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}
