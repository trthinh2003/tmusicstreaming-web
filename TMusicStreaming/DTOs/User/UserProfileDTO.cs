namespace TMusicStreaming.DTOs.User
{
    public class UserProfileDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Gender { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
