using Data_Organizer_Server.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Data_Organizer_Server.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _keyBytes;
        private readonly byte[] _ivBytes;
        private readonly ILogger<EncryptionService> _logger;

        public EncryptionService(ILogger<EncryptionService> logger)
        {
            _logger = logger;

            string keyBase64 = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
            string ivBase64 = Environment.GetEnvironmentVariable("ENCRYPTION_IV");

            if (string.IsNullOrWhiteSpace(keyBase64) || string.IsNullOrWhiteSpace(ivBase64))
            {
                _logger.LogError("Encryption key or IV not found in environment variables.");
                throw new InvalidOperationException("Missing encryption key or IV in environment variables.");
            }

            try
            {
                _keyBytes = Convert.FromBase64String(keyBase64);
                _ivBytes = Convert.FromBase64String(ivBase64);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Failed to parse base64 encryption key or IV.");
                throw;
            }
        }

        public string Encrypt(string plainText)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText))
                {
                    _logger.LogWarning("Attempt to encrypt null or empty string.");
                    return null;
                }

                using var aes = Aes.Create();
                aes.Key = _keyBytes;
                aes.IV = _ivBytes;

                using var encryptor = aes.CreateEncryptor();
                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs, Encoding.UTF8))
                {
                    sw.Write(plainText);
                }

                var encryptedBytes = ms.ToArray();
                var result = Convert.ToBase64String(encryptedBytes);

                _logger.LogDebug("Encryption successful.");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Encryption failed.");
                return null;
            }
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                if (string.IsNullOrEmpty(cipherText))
                {
                    _logger.LogWarning("Attempt to decrypt null or empty string.");
                    return null;
                }

                var cipherBytes = Convert.FromBase64String(cipherText);

                using var aes = Aes.Create();
                aes.Key = _keyBytes;
                aes.IV = _ivBytes;

                using var decryptor = aes.CreateDecryptor();
                using var ms = new MemoryStream(cipherBytes);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs, Encoding.UTF8);

                var plainText = sr.ReadToEnd();

                _logger.LogDebug("Decryption successful.");
                return plainText;
            }
            catch (CryptographicException ex)
            {
                _logger.LogWarning(ex, "Decryption failed. Possibly due to incorrect key/IV or corrupted data.");
                return null;
            }
            catch (FormatException ex)
            {
                _logger.LogWarning(ex, "Invalid format of input string for decryption.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during decryption.");
                return null;
            }
        }
    }

}
