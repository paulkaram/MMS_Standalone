using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Intalio.Tools.Common.JwtToken
{
    public class JwtTokenGenerator
    {
        private readonly JwtSettings _settings;

        public JwtTokenGenerator(JwtSettings settings)
        {
            _settings = settings;
        }

        public string GenerateToken(int originalUserId, string userId, string structureId, string userFullName, string? language = null)
        {
            var claims = new ClaimsIdentity(new[] {
                new Claim(CommonClaimNames.UserId, userId),
                new Claim(CommonClaimNames.StructureId, structureId),
                new Claim(CommonClaimNames.FullName, userFullName),
                new Claim(ClaimTypes.NameIdentifier, originalUserId.ToString()),//NameIdentifier: is used by signalr to map connection ids to users
            });

            if (!string.IsNullOrWhiteSpace(language))
            {
                claims.AddClaim(new(CommonClaimNames.Language, language));
            }

            return GenerateToken(claims.Claims.ToArray());
        }

        public string GenerateToken(string userId, params string[] roles)
        {
            var claims = new ClaimsIdentity(new[] {
                new Claim(CommonClaimNames.UserId, userId)
            });

            foreach (string role in roles)
            {
                claims.AddClaim(new(ClaimTypes.Role, role));
            }

            return GenerateToken(claims.Claims.ToArray());
        }

        public string GenerateToken(params Claim[] claims)
        {
            var symetricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_settings.Secret));

            var securityToken = new JwtSecurityToken(
                issuer: !string.IsNullOrWhiteSpace(_settings.Issuer) ? _settings.Issuer : null,
                audience: !string.IsNullOrWhiteSpace(_settings.Audience) ? _settings.Audience : null,
                claims: claims,
                expires: _settings.ExpiryMinutes.HasValue ? DateTime.Now.AddMinutes(_settings.ExpiryMinutes.Value) : null,
                // NCA Compliance: Use HMAC-SHA384 for JWT signing (NCS-1:2020 Section 4.4)
                signingCredentials: new SigningCredentials(symetricSecurityKey, SecurityAlgorithms.HmacSha384)
            );

            var handler = new JwtSecurityTokenHandler()
            {
                SetDefaultTimesOnTokenCreation = !_settings.OmitDefaultTimesOnTokenCreation
            };

            return handler.WriteToken(securityToken);
        }

        public bool ValidateToken(string jwtToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));

            var validationParameters = new TokenValidationParameters
            {
                RequireExpirationTime = _settings.ExpiryMinutes.HasValue,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,

                ValidateIssuer = !string.IsNullOrWhiteSpace(_settings.Issuer),
                ValidIssuer = !string.IsNullOrWhiteSpace(_settings.Issuer) ? _settings.Issuer : null,

                ValidateAudience = !string.IsNullOrWhiteSpace(_settings.Audience),
                ValidAudience = !string.IsNullOrWhiteSpace(_settings.Audience) ? _settings.Audience : null,

                ClockSkew = TimeSpan.Zero
            };

            try
            {
                // Validate the JWT token
                tokenHandler.ValidateToken(jwtToken, validationParameters, out _);
                return true;
            }
            catch (Exception)
            {
                // Token validation failed
                return false;
            }
        }

        public static string? GetClaim(string jwtToken, string claimName)
        {
            return new JwtSecurityTokenHandler()
                .ReadJwtToken(jwtToken).Claims
                .FirstOrDefault(claim => claim.Type == claimName)?.Value;
        }

        public static class CommonClaimNames
        {
            public const string Language = "lang";
            public const string UserId = "account";
            public const string FullName = "name";
            public const string StructureId = "department";
            public const string ProfilePictureUrl = "profilePictureUrl";
            public const string NationlId = "nationalId";

		}
    }


}