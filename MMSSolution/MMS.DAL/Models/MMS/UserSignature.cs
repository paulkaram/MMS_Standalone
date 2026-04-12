using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class UserSignature
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public byte[] Signature { get; set; } = null!;

    public int? Pincode { get; set; }

    public int TypeId { get; set; }

    public int FailedAttempts { get; set; }

    public bool IsLocked { get; set; }

    public DateTime? LastAttempt { get; set; }

    public DateTime? LastSuccessfulAttempt { get; set; }

    public virtual SignatureType Type { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
