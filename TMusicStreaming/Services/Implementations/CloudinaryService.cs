using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Services.Implementations
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly ILogger<CloudinaryService> _logger;
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(ILogger<CloudinaryService> logger, IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"] ?? string.Empty,
                config["Cloudinary:ApiKey"] ?? string.Empty,
                config["Cloudinary:ApiSecret"] ?? string.Empty
            );
            _cloudinary = new Cloudinary(account);
            _logger = logger;
        }

        public async Task<UsageResult> GetUsageAsync()
        {
            try
            {
                var result = await _cloudinary.GetUsageAsync();
                _logger.LogInformation("Lấy thông tin sử dụng Cloudinary thành công");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin sử dụng Cloudinary");
                throw;
            }
        }

        public async Task<string?> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return null;
            var fileName = file.FileName ?? "unknown";
            var extension = Path.GetExtension(fileName).ToLower();
            _logger.LogInformation("Uploading file: {FileName}, Extension: {Extension}, ContentType: {ContentType}",
                fileName, extension, file.ContentType);

            if (string.IsNullOrEmpty(extension))
            {
                extension = GetExtensionFromContentType(file.ContentType);
                _logger.LogInformation("Extension detected from ContentType: {Extension}", extension);
            }

            if (string.IsNullOrEmpty(extension))
            {
                _logger.LogError("Không thể xác định định dạng file: FileName='{FileName}', ContentType='{ContentType}'",
                    fileName, file.ContentType);
                return null;
            }

            var uniqueName = Guid.NewGuid().ToString();
            var fileId = $"{folderName}/{uniqueName}{extension}";

            await using var stream = file.OpenReadStream();

            if (extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".svg")
            {
                var imageParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    PublicId = fileId,
                    Folder = folderName
                };
                var result = await _cloudinary.UploadAsync(imageParams);
                return result.SecureUrl?.ToString();
            }
            else if (extension is ".lrc" or ".txt")
            {
                var rawParams = new RawUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    PublicId = fileId,
                    Folder = folderName
                };
                var result = await _cloudinary.UploadAsync(rawParams);
                return result.SecureUrl?.ToString();
            }
            else if (extension is ".mp3" or ".wav" or ".ogg" or ".aac" or ".flac")
            {
                var videoParams = new VideoUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    PublicId = fileId,
                    Folder = folderName
                };
                var result = await _cloudinary.UploadAsync(videoParams);
                return result.SecureUrl?.ToString();
            }
            else
            {
                _logger.LogError("Định dạng file không được hỗ trợ: {Extension} (File: {FileName})", extension, fileName);
                return null;
            }
        }

        private string GetExtensionFromContentType(string? contentType)
        {
            return contentType?.ToLower() switch
            {
                "text/plain" => ".txt",
                "application/x-subrip" => ".lrc",
                "audio/mpeg" => ".mp3",
                "audio/wav" => ".wav",
                "audio/wave" => ".wav",
                "audio/ogg" => ".ogg",
                "audio/aac" => ".aac",
                "audio/flac" => ".flac",
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "image/svg+xml" => ".svg",
                _ => string.Empty
            };
        }

        public async Task<bool> DeleteFileAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
            {
                _logger.LogWarning("Không thể xóa file với publicId rỗng.");
                return false;
            }

            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);

            if (result.Result == "ok")
            {
                _logger.LogInformation("Đã xóa file Cloudinary: {PublicId}", publicId);
                return true;
            }

            _logger.LogError("Xóa file thất bại: {PublicId}, Lý do: {Reason}", publicId, result.Error?.Message);
            return false;
        }
    }
}