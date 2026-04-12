namespace MMS.DTO.Permissions;

public class IamGroupDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public bool IsActive { get; set; }
}

public class IamRoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public bool IsActive { get; set; }
}

public class MenuPermissionItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public bool IsAssigned { get; set; }
}

public class SaveMenuPermissionsRequest
{
    public string? DisplayName { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}
