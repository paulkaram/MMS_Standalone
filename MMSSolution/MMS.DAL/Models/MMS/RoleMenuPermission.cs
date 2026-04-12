using System;

namespace MMS.DAL.Models.MMS;

public class RoleMenuPermission
{
    public int Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int PermissionId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public virtual Permission? Permission { get; set; }
}
