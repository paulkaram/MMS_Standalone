using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class SignatureType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<UserSignature> UserSignatures { get; set; } = new List<UserSignature>();
}
