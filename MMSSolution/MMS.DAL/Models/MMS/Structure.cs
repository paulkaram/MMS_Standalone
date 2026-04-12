using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class Structure
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public int TypeId { get; set; }

    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public bool Active { get; set; }

    public bool ExternalStructure { get; set; }

    public DateTime CreatedDate { get; set; }

    public int Sort { get; set; }

    public int? BranchId { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<Structure> InverseParent { get; set; } = new List<Structure>();

    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();

    public virtual Structure? Parent { get; set; }

    public virtual ICollection<PermissionMatrix> PermissionMatrices { get; set; } = new List<PermissionMatrix>();

    public virtual StructureType Type { get; set; } = null!;

    public virtual ICollection<UserStructure> UserStructures { get; set; } = new List<UserStructure>();
}
