using System.Security.Cryptography;
using System.Text;

namespace Intalio.Tools.Common.Encryptions
{
	/// <summary>
	/// AES encryption service with secure key management.
	/// NCA Compliance: Keys are loaded from configuration, not hardcoded (NCS-1:2020 Section 8).
	/// </summary>
	public class EncryptionServiceImpl : IEncryptionService
	{
		private readonly byte[] _key;
		private readonly byte[] _iv;
		private readonly byte[] _frontendKey;
		private readonly byte[] _frontendIv;

		public EncryptionServiceImpl(EncryptionSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			if (string.IsNullOrEmpty(settings.Key))
				throw new ArgumentException("Encryption key is required. Configure 'Encryption:Key' in environment variables or secure configuration.", nameof(settings));

			if (string.IsNullOrEmpty(settings.IV))
				throw new ArgumentException("Encryption IV is required. Configure 'Encryption:IV' in environment variables or secure configuration.", nameof(settings));

			_key = Encoding.UTF8.GetBytes(settings.Key);
			_iv = Encoding.UTF8.GetBytes(settings.IV);
			_frontendKey = Encoding.UTF8.GetBytes(settings.FrontendKey ?? settings.Key);
			_frontendIv = Encoding.UTF8.GetBytes(settings.FrontendIV ?? settings.IV);
		}

		public string Encrypt(string plaintext)
		{
			return Encrypt(plaintext, _key, _iv);
		}

		public string Decrypt(string cypherText)
		{
			return Decrypt(cypherText, _key, _iv);
		}

		public string DecryptFromFrontend(string cypherText)
		{
			using Aes aes = Aes.Create();
			aes.Key = _frontendKey;
			aes.IV = _frontendIv;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.None;

			using MemoryStream memoryStream = new();
			using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);

			byte[] encryptedBytes = Convert.FromBase64String(cypherText);
			cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
			cryptoStream.FlushFinalBlock();

			byte[] decryptedBytes = memoryStream.ToArray();

			// Remove zero padding
			int padIndex = decryptedBytes.Length;
			for (int i = decryptedBytes.Length - 1; i >= 0; i--)
			{
				if (decryptedBytes[i] != 0)
				{
					padIndex = i + 1;
					break;
				}
			}

			return Encoding.UTF8.GetString(decryptedBytes, 0, padIndex);
		}

		private static string Encrypt(string plaintext, byte[] secretkey, byte[] vector)
		{
			using Aes aes = Aes.Create();
			aes.Key = secretkey;
			aes.IV = vector;

			using MemoryStream memoryStream = new();
			using CryptoStream cryptoStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

			cryptoStream.Write(Encoding.UTF8.GetBytes(plaintext), 0, plaintext.Length);
			cryptoStream.FlushFinalBlock();

			return Convert.ToBase64String(memoryStream.ToArray());
		}

		private static string Decrypt(string cypherText, byte[] secretkey, byte[] vector)
		{
			using Aes aes = Aes.Create();
			aes.Key = secretkey;
			aes.Mode = CipherMode.CBC;
			aes.IV = vector;

			using MemoryStream memoryStream = new();
			using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);

			var encryptedBytes = Convert.FromBase64String(cypherText);

			cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
			cryptoStream.FlushFinalBlock();

			return Encoding.UTF8.GetString(memoryStream.ToArray());
		}
	}

	/// <summary>
	/// Static encryption service for backwards compatibility.
	/// WARNING: This class uses keys from configuration. Ensure EncryptionService.Initialize() is called at startup.
	/// </summary>
	[Obsolete("Use IEncryptionService via dependency injection for NCA compliance. This static class is provided for backwards compatibility only.")]
	public static class EncryptionService
	{
		private static byte[]? _key;
		private static byte[]? _iv;
		private static byte[]? _frontendKey;
		private static byte[]? _frontendIv;
		private static bool _initialized = false;

		/// <summary>
		/// Initialize the static encryption service with settings from configuration.
		/// Must be called at application startup before using any encryption methods.
		/// </summary>
		public static void Initialize(EncryptionSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			if (string.IsNullOrEmpty(settings.Key))
				throw new ArgumentException("Encryption key is required. Configure 'Encryption:Key' in environment variables or secure configuration.", nameof(settings));

			if (string.IsNullOrEmpty(settings.IV))
				throw new ArgumentException("Encryption IV is required. Configure 'Encryption:IV' in environment variables or secure configuration.", nameof(settings));

			_key = Encoding.UTF8.GetBytes(settings.Key);
			_iv = Encoding.UTF8.GetBytes(settings.IV);
			_frontendKey = Encoding.UTF8.GetBytes(settings.FrontendKey ?? settings.Key);
			_frontendIv = Encoding.UTF8.GetBytes(settings.FrontendIV ?? settings.IV);
			_initialized = true;
		}

		private static void EnsureInitialized()
		{
			if (!_initialized || _key == null || _iv == null)
			{
				throw new InvalidOperationException(
					"EncryptionService has not been initialized. " +
					"Call EncryptionService.Initialize(settings) at application startup with encryption settings from secure configuration. " +
					"NCA Compliance: Encryption keys must not be hardcoded (NCS-1:2020 Section 8).");
			}
		}

		public static string Encrypt(string plaintext)
		{
			EnsureInitialized();
			return Encrypt(plaintext, _key!, _iv!);
		}

		public static string Encrypt(string plaintext, byte[] secretkey, byte[] vector)
		{
			using Aes aes = Aes.Create();
			aes.Key = secretkey;
			aes.IV = vector;

			using MemoryStream memoryStream = new();
			using CryptoStream cryptoStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

			cryptoStream.Write(Encoding.UTF8.GetBytes(plaintext), 0, plaintext.Length);
			cryptoStream.FlushFinalBlock();

			return Convert.ToBase64String(memoryStream.ToArray());
		}

		public static string Decrypt(string cypherText)
		{
			EnsureInitialized();
			return Decrypt(cypherText, _key!, _iv!);
		}

		public static string DecryptFromFrontend(string cypherText)
		{
			EnsureInitialized();
			using Aes aes = Aes.Create();
			aes.Key = _frontendKey!;
			aes.IV = _frontendIv!;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.None;

			using MemoryStream memoryStream = new();
			using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);

			byte[] encryptedBytes = Convert.FromBase64String(cypherText);
			cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
			cryptoStream.FlushFinalBlock();

			byte[] decryptedBytes = memoryStream.ToArray();

			// Remove zero padding
			int padIndex = decryptedBytes.Length;
			for (int i = decryptedBytes.Length - 1; i >= 0; i--)
			{
				if (decryptedBytes[i] != 0)
				{
					padIndex = i + 1;
					break;
				}
			}

			return Encoding.UTF8.GetString(decryptedBytes, 0, padIndex);
		}

		public static string Decrypt(string cypherText, byte[] secretkey, byte[] vector)
		{
			using Aes aes = Aes.Create();
			aes.Key = secretkey;
			aes.Mode = CipherMode.CBC;
			aes.IV = vector;

			using MemoryStream memoryStream = new();
			using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);

			var encryptedBytes = Convert.FromBase64String(cypherText);

			cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
			cryptoStream.FlushFinalBlock();

			return Encoding.UTF8.GetString(memoryStream.ToArray());
		}
	}
}
