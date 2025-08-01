using CloudinaryDotNet.Actions;

namespace TMusicStreaming.Services.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string?> UploadFileAsync(IFormFile file, string folderName);
        Task<bool> DeleteFileAsync(string publicId);
        Task<UsageResult> GetUsageAsync();
    }
}
