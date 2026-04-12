namespace MMS.DTO.Oidc
{
	public record OidcSetting(string? Issuer = null, string? Audiences = null, string? Authority = null, string? SigningKey = null, bool? HttpsOnly = false, bool? SaveToken = false, string? PublickeysUrl = null, int? ClockSkew = 0);

}
