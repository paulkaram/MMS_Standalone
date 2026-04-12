using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class Language
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
