namespace MMS.DTO.Permissions
{
    public class SecondLevelPermissionDto
    {
        public string? GroupName { get; set; }

        public List<PermissionAccessListItemDto>? Items { get; set; }
    }
}
