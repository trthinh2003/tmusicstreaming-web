using TMusicStreaming.DTOs.User;
using TMusicStreaming.Repositories.Interfaces;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ICloudinaryService cloudinaryService, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        public async Task<UserProfileDTO?> GetUserProfileAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            return new UserProfileDTO
            {
                Id = user.Id,
                Name = user.Name,
                Avatar = user.Avatar,
                UserName = user.UserName,
                Email = user.Email,
                Gender = user.Gender,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserProfileDTO?> UpdateUserProfileAsync(int userId, UpdateUserProfileRequest request, IFormFile? avatarFile)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found for update.");
                return null;
            }

            // Kiểm tra email trùng lặp nếu email thay đổi
            if (user.Email != request.Email)
            {
                var existingUserWithEmail = await _userRepository.GetUserByEmailAsync(request.Email);
                if (existingUserWithEmail != null && existingUserWithEmail.Id != userId)
                {
                    _logger.LogWarning($"Email '{request.Email}' already exists for another user.");
                    throw new InvalidOperationException("Email này đã được sử dụng bởi người dùng khác.");
                }
            }

            // Cập nhật thông tin cơ bản
            user.Name = request.Name;
            user.Email = request.Email;
            user.Gender = request.Gender;

            // Xử lý avatar
            if (avatarFile != null)
            {
                if (!string.IsNullOrEmpty(user.Avatar)) // Kiểm tra avatar cũ trên Cloudinary => nếu có thì xóa
                {
                    var oldPublicId = ExtractPublicIdFromCloudinaryUrl(user.Avatar);
                    if (!string.IsNullOrEmpty(oldPublicId))
                    {
                        await _cloudinaryService.DeleteFileAsync(oldPublicId);
                    }
                }

                var uploadedAvatarUrl = await _cloudinaryService.UploadFileAsync(avatarFile, "TMusicStreaming/users/avatars");
                if (uploadedAvatarUrl == null)
                {
                    _logger.LogError("Failed to upload new avatar for user {UserId}", userId);
                }
                else
                {
                    user.Avatar = uploadedAvatarUrl;
                }
            }
            else if (request.AvatarUrl == "") // Nếu AvatarUrl trong request là chuỗi rỗng, nghĩa là người dùng muốn xóa avatar
            {
                if (!string.IsNullOrEmpty(user.Avatar))
                {
                    var oldPublicId = ExtractPublicIdFromCloudinaryUrl(user.Avatar);
                    if (!string.IsNullOrEmpty(oldPublicId))
                    {
                        await _cloudinaryService.DeleteFileAsync(oldPublicId);
                    }
                }
                user.Avatar = string.Empty;
            }
            var updatedUser = await _userRepository.UpdateUserAsync(user);
            if (updatedUser == null)
            {
                _logger.LogError("Failed to save updated user profile for user {UserId}", userId);
                return null;
            }

            return new UserProfileDTO
            {
                Id = updatedUser.Id,
                Name = updatedUser.Name,
                Avatar = updatedUser.Avatar,
                UserName = updatedUser.UserName,
                Email = updatedUser.Email,
                Gender = updatedUser.Gender,
                Role = updatedUser.Role,
                CreatedAt = updatedUser.CreatedAt
            };
        }

        // Helper để trích xuất public ID từ URL của Cloudinary
        // Ví dụ: https://res.cloudinary.com/dny7pcxme/image/upload/v123456789/avatars/unique_id.jpg
        // Public ID sẽ là "avatars/unique_id.jpg" (không bao gồm phần mở rộng nếu Cloudinary tự động thêm vào)
        private string? ExtractPublicIdFromCloudinaryUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            // Tìm vị trí của "upload/" hoặc "upload/v"
            var uploadIndex = url.IndexOf("/upload/");
            if (uploadIndex == -1)
            {
                uploadIndex = url.IndexOf("/upload/v"); // handle versioned URLs
                if (uploadIndex == -1) return null;
            }

            // Lấy phần còn lại của URL sau "upload/" hoặc "upload/v[version_number]/"
            var potentialPublicId = url.Substring(uploadIndex + "/upload/".Length);

            // Xóa phần version number nếu có (ví dụ: v123456789/)
            var versionSlashIndex = potentialPublicId.IndexOf('/');
            if (versionSlashIndex != -1 && potentialPublicId.StartsWith("v") && int.TryParse(potentialPublicId.Substring(1, versionSlashIndex - 1), out _))
            {
                potentialPublicId = potentialPublicId.Substring(versionSlashIndex + 1);
            }

            // Xóa phần mở rộng cuối cùng (ví dụ: .jpg, .png)
            var lastDotIndex = potentialPublicId.LastIndexOf('.');
            if (lastDotIndex != -1)
            {
                potentialPublicId = potentialPublicId.Substring(0, lastDotIndex);
            }

            // Xóa bất kỳ biến đổi nào (ví dụ: /c_fill,h_150,w_150/)
            var transformationEndIndex = potentialPublicId.LastIndexOf('/');
            if (transformationEndIndex != -1)
            {
                var folderPart = potentialPublicId.Substring(0, transformationEndIndex);
                var filePart = potentialPublicId.Substring(transformationEndIndex + 1);

                // Kiểm tra xem folderPart có phải là một transformation string không (ví dụ: c_fill,h_150,w_150)
                if (folderPart.Contains("_") && !folderPart.Contains("/"))
                { // Kiểm tra đơn giản để xác định có phải transformation không
                    // Nếu là transformation, giữ nguyên publicId, Cloudinary sẽ xử lý khi xóa
                }
                else
                {
                    return potentialPublicId; // Trả về publicId đã loại bỏ mở rộng và version
                }
            }

            return potentialPublicId; // Trả về publicId đã loại bỏ mở rộng và version
        }
    }
}
