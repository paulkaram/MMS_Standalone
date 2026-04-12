using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class DataSource
{
    public int Id { get; set; }

    public string Dbname { get; set; } = null!;

    public string InstanceName { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;
}
