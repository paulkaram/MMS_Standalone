using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class RefreshToken
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Token { get; set; } = null!;

    public DateTime Expiration { get; set; }

    public virtual User User { get; set; } = null!;
}
