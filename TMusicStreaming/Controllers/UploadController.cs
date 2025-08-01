using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TMusicStreaming.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IConfiguration config, ILogger<UploadController> logger)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, string folder)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("File không hợp lệ");

                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                var uniqueFileName = Guid.NewGuid().ToString();
                var folderPath = $"TMusicStreaming/{folder}"; // Thay đổi thư mục đích
                var fileId = $"{folderPath}/{uniqueFileName}"; // Định danh file trong thư mục

                using var stream = file.OpenReadStream();
                UploadResult uploadResult;

                if (fileExtension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp")
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        PublicId = fileId,
                        Folder = folderPath
                    };
                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                }
                else if (fileExtension is ".lrc" or ".txt")
                {
                    var uploadParams = new RawUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        PublicId = fileId,
                        Folder = folderPath
                    };
                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                }
                else if (fileExtension is ".mp3" or ".wav" or ".ogg")
                {
                    var uploadParams = new VideoUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        PublicId = fileId,
                        Folder = folderPath
                    };
                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                }
                else
                {
                    return BadRequest("Định dạng file không hỗ trợ!");
                }

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Lỗi khi tải lên Cloudinary: {0}", uploadResult.Error.Message);
                    return StatusCode(500, $"Lỗi Cloudinary: {uploadResult.Error.Message}");
                }

                _logger.LogInformation("Tệp đã tải lên thành công: {0}", uploadResult.SecureUrl);
                return Ok(new { url = uploadResult.SecureUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi hệ thống khi tải lên tệp: {0}", ex.Message);
                return BadRequest(new { message = "Lỗi khi tải lên tệp: " + ex.Message });
            }
        }

    }
}
