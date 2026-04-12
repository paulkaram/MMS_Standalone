namespace MMS.DTO.Users;

public class IamUserListItemDto
{
	public string Id { get; set; } = string.Empty;
	public string Username { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string DisplayName { get; set; } = string.Empty;
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public bool IsActive { get; set; }
	public string? PhoneNumber { get; set; }
}
