namespace Intalio.Tools.Common.Encryptions
{
    /// <summary>
    /// Interface for encryption services.
    /// NCA Compliance: Implementations must use secure key management (NCS-1:2020 Section 8).
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts plaintext using AES encryption with configured keys.
        /// </summary>
        string Encrypt(string plaintext);

        /// <summary>
        /// Decrypts ciphertext using AES encryption with configured keys.
        /// </summary>
        string Decrypt(string cypherText);

        /// <summary>
        /// Decrypts ciphertext that was encrypted by the frontend.
        /// </summary>
        string DecryptFromFrontend(string cypherText);
    }
}
