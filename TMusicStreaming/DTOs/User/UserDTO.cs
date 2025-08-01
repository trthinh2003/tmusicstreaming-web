using System.ComponentModel.DataAnnotations;

namespace TMusicStreaming.DTOs.User
{
    public class UserDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool Gender { get; set; }

        public string Avatar { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        //[MaxLength(255)]
        //public string PasswordHash { get; set; } = string.Empty;
    }
}
