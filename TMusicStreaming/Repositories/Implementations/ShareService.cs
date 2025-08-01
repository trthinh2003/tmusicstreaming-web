using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TMusicStreaming.Repositories.Interfaces;

namespace TMusicStreaming.Repositories.Implementations
{
    public class ShareService : IShareService
    {
        private readonly string _secretKey;
        private readonly ILogger<ShareService> _logger;

        public ShareService(IConfiguration configuration, ILogger<ShareService> logger)
        {
            _secretKey = configuration["ShareService:SecretKey"] ?? "";
            _logger = logger;
        }

        public string CreateShareLink(int songId, int expireInMinutes = 60)
        {
            try
            {
                var shareData = new
                {
                    songId = songId,
                    expireAt = DateTime.UtcNow.AddMinutes(expireInMinutes),
                    createdAt = DateTime.UtcNow
                };

                var jsonData = JsonSerializer.Serialize(shareData);
                _logger.LogInformation("Creating share link - JSON data: {JsonData}", jsonData);

                var encryptedData = EncryptString(jsonData);
                _logger.LogInformation("Encrypted data: {EncryptedData}", encryptedData);

                // Sử dụng WebEncoders để encode URL-safe
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(encryptedData));

                _logger.LogInformation("Created share link for song {SongId}, token: {Token}", songId, encodedToken);

                return encodedToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating share link for song {SongId}", songId);
                throw;
            }
        }

        public (int? songId, bool isValid, DateTime? expireAt, DateTime? createdAt) ValidateShareLink(string token)
        {
            try
            {
                _logger.LogInformation("Validating share token: {Token}", token);

                // Decode URL-safe base64 - KHÔNG CẦN thêm padding thủ công
                byte[] tokenBytes;
                try
                {
                    tokenBytes = WebEncoders.Base64UrlDecode(token);
                    _logger.LogInformation("Successfully decoded token to bytes, length: {Length}", tokenBytes.Length);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to decode token: {Token}", token);
                    return (null, false, null, null);
                }

                var encryptedData = Encoding.UTF8.GetString(tokenBytes);
                _logger.LogInformation("Encrypted data from token: {EncryptedData}", encryptedData);

                string decryptedJson;
                try
                {
                    decryptedJson = DecryptString(encryptedData);
                    _logger.LogInformation("Decrypted JSON: {DecryptedJson}", decryptedJson);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to decrypt data");
                    return (null, false, null, null);
                }

                ShareData shareData;
                try
                {
                    shareData = JsonSerializer.Deserialize<ShareData>(decryptedJson);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize JSON: {Json}", decryptedJson);
                    return (null, false, null, null);
                }

                if (shareData == null)
                {
                    _logger.LogWarning("Invalid share data for token: {Token}", token);
                    return (null, false, null, null);
                }

                var isValid = DateTime.UtcNow < shareData.ExpireAt;

                _logger.LogInformation("Share token validation result - SongId: {SongId}, IsValid: {IsValid}, ExpireAt: {ExpireAt}",
                    shareData.SongId, isValid, shareData.ExpireAt);

                return (shareData.SongId, isValid, shareData.ExpireAt, shareData.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating share link for token: {Token}", token);
                return (null, false, null, null);
            }
        }

        private string EncryptString(string plainText)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(_secretKey.PadRight(32).Substring(0, 32));
                    aes.IV = new byte[16]; // Zero IV for simplicity

                    using (var encryptor = aes.CreateEncryptor())
                    {
                        var plainBytes = Encoding.UTF8.GetBytes(plainText);
                        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                        return Convert.ToBase64String(encryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting string: {PlainText}", plainText);
                throw;
            }
        }

        private string DecryptString(string cipherText)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(_secretKey.PadRight(32).Substring(0, 32));
                    aes.IV = new byte[16]; // Zero IV for simplicity

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        var encryptedBytes = Convert.FromBase64String(cipherText);
                        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting string: {CipherText}", cipherText);
                throw;
            }
        }

        private class ShareData
        {
            [JsonPropertyName("songId")] 
            public int SongId { get; set; }

            [JsonPropertyName("expireAt")]  
            public DateTime ExpireAt { get; set; }

            [JsonPropertyName("createdAt")]  
            public DateTime CreatedAt { get; set; }
        }
    }
}