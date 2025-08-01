using TMusicStreaming.DTOs.User;
using TMusicStreaming.Models;

namespace TMusicStreaming.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<List<UserDTO>> GetAllUserAsync();
        Task<List<UserDTO>> SearchUsersAsync(string searchQuery);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<int> AddUserAsync(UserDTO user);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> SaveChangesAsync();
    }
}
