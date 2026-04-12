namespace MMS.DTO.Users.Auth
{
	public class LoginOptionsDto
	{
		public bool EnableLogin { get; set; }
		public bool EnableExternalLogin { get; set; }
		public string? ExternalTokenName { get; set; }
	}
}
