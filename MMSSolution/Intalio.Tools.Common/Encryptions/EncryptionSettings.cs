namespace Intalio.Tools.Common.Encryptions
{
    /// <summary>
    /// Configuration settings for encryption service.
    /// NCA Compliance: Keys must be stored securely and not hardcoded (NCS-1:2020 Section 8).
    /// Configure these values in environment variables or a secure vault, not in appsettings.json.
    /// </summary>
    public class EncryptionSettings
    {
        /// <summary>
        /// AES encryption key (32 characters for 256-bit key)
        /// </summary>
        public string Key { get; set; } = null!;

        /// <summary>
        /// AES initialization vector (16 characters for 128-bit IV)
        /// </summary>
        public string IV { get; set; } = null!;

        /// <summary>
        /// Frontend encryption key (for decrypting data from frontend)
        /// </summary>
        public string FrontendKey { get; set; } = null!;

        /// <summary>
        /// Frontend initialization vector
        /// </summary>
        public string FrontendIV { get; set; } = null!;
    }
}
