using System.Security.Cryptography;
using System.Text;

namespace Intalio.Tools.Common
{
	public static class StringManipulation
	{
		private const string IntalioRandomCharacters = "Intalio-KSA@2023";

		/// <summary>
		/// NCA Compliant hash function using SHA2-384 (NCS-1:2020 Section 4.1)
		/// </summary>
		public static string SHA_384(params string[] inputs)
		{
			StringBuilder oResHash = new();
			Encoding oEnc = Encoding.UTF8;
			byte[] baResult = SHA384.HashData(oEnc.GetBytes($"{string.Join("", inputs)}"));
			foreach (byte b in baResult)
			{
				oResHash.Append(b.ToString("X2"));
			}
			return oResHash.ToString();
		}

		/// <summary>
		/// Deprecated: Use SHA_384 instead for NCA compliance (NCS-1:2020 Section 4.1)
		/// SHA-256 is not in the NCA accepted hash functions list
		/// </summary>
		[Obsolete("Use SHA_384 instead for NCA compliance. SHA-256 is not in the NCA accepted hash functions list.")]
		public static string SHA_256(params string[] inputs)
		{
			StringBuilder oResHash = new();
			Encoding oEnc = Encoding.UTF8;
			byte[] baResult = SHA256.HashData(oEnc.GetBytes($"{string.Join("", inputs)}"));
			foreach (byte b in baResult)
			{
				oResHash.Append(b.ToString("X2"));
			}
			return oResHash.ToString();
		}

		public static string ExtendStringValue(string? value)
		{
			// NCA Compliance: Use cryptographically secure random number generator (NCS-1:2020 Section 7.1)
			string randomNumber = RandomNumberGenerator.GetInt32(10000, 90000).ToString();
			return value!= null ? $"{randomNumber}{IntalioRandomCharacters}{value}" : string.Empty;
		}

		public static string ContractStringValue(string? value)
		{
			if (value != null && value.Contains(IntalioRandomCharacters))
			{
				return value.Split(IntalioRandomCharacters)[1];
			}

			return value ?? "";
		}

		/// <summary>
		/// Verifies a password against a stored hash, supporting both legacy SHA-256 and NCA-compliant SHA-384.
		/// SHA-256 hashes are 64 hex characters, SHA-384 hashes are 96 hex characters.
		/// </summary>
		/// <param name="salt">The salt used for hashing</param>
		/// <param name="password">The password to verify</param>
		/// <param name="storedHash">The stored hash to verify against</param>
		/// <returns>True if password matches, false otherwise</returns>
		public static bool VerifyPassword(string salt, string password, string storedHash)
		{
			if (string.IsNullOrEmpty(storedHash))
				return false;

			// SHA-256 produces 64 hex characters, SHA-384 produces 96 hex characters
			if (storedHash.Length == 96)
			{
				// NCA-compliant SHA-384 hash
				return SHA_384(salt, password) == storedHash;
			}
			else
			{
				// Legacy SHA-256 hash (64 characters)
#pragma warning disable CS0618 // Type or member is obsolete
				return SHA_256(salt, password) == storedHash;
#pragma warning restore CS0618
			}
		}

		/// <summary>
		/// Checks if a password hash needs migration to NCA-compliant SHA-384.
		/// </summary>
		/// <param name="storedHash">The stored hash to check</param>
		/// <returns>True if migration is needed (hash is legacy SHA-256)</returns>
		public static bool NeedsHashMigration(string? storedHash)
		{
			// SHA-384 produces 96 hex characters, SHA-256 produces 64
			return storedHash != null && storedHash.Length != 96;
		}

	}
}
