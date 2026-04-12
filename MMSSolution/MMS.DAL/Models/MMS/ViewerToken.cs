using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class ViewerToken
{
    public int Id { get; set; }

    public string Token { get; set; } = null!;

    public string Username { get; set; } = null!;
}
