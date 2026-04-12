namespace MMS.DTO.Permissions
{
	public record UpdatePermissionDto(int Order,bool IsDefault,string Description,int? MapId);
}
