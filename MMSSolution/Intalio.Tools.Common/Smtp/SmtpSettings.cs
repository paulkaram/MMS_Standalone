namespace Intalio.Tools.Common.Smtp
{
	public class SmtpSettings
	{
		public string? Host { get; set; }
		public int Port { get; set; }
		public string User { get; set; } = null!;
		public string FromEmail { get; set; } = null!;
		public string Password { get; set; } = null!;
		public bool EnableSSL { get; set; }
	}
}
