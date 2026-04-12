using System;

namespace MMS.DAL.Models.MMS;

public class GroupMenuPermission
{
    public int Id { get; set; }
    public string GroupId { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public int PermissionId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public virtual Permission? Permission { get; set; }
}
