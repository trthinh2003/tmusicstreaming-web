using System.ComponentModel.DataAnnotations;

namespace TMusicStreaming.DTOs.User
{
    public class UpdateUserProfileRequest
    {
        [Required(ErrorMessage = "Tên không được để trống.")]
        [MaxLength(255, ErrorMessage = "Tên không được vượt quá 255 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [MaxLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự.")]
        public string Email { get; set; } = string.Empty;

        public bool Gender { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
