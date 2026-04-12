namespace Intalio.Tools.Common.JwtToken
{
	public class JwtSettings
	{
		public string? Issuer { get; set; }
		public string? Audience { get; set; }
		public string Secret { get; set; } = null!;
		public double? ExpiryMinutes { get; set; }
		public double? RefreshExpiryMinutes { get; set; }
		public bool OmitDefaultTimesOnTokenCreation { get; set; }
	}
}
