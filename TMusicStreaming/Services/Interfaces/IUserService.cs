using TMusicStreaming.DTOs.User;

namespace TMusicStreaming.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDTO?> GetUserProfileAsync(int userId);
        Task<UserProfileDTO?> UpdateUserProfileAsync(int userId, UpdateUserProfileRequest request, IFormFile? avatarFile);
    }
}
