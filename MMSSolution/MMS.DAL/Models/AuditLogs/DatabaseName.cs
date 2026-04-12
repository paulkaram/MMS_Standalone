using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.AuditLogs;

public partial class DatabaseName
{
    public int Id { get; set; }

    public string Dbname { get; set; } = null!;
}
