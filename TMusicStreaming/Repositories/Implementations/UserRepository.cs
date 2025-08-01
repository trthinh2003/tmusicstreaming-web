using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TMusicStreaming.Data;
using TMusicStreaming.DTOs.User;
using TMusicStreaming.Models;
using TMusicStreaming.Repositories.Interfaces;

namespace TMusicStreaming.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly TMusicStreamingContext _context;
        private readonly IMapper _mapper;

        public UserRepository(TMusicStreamingContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<UserDTO>> GetAllUserAsync()
        {
            try
            {
                var users = await _context.Users!
                    .Where(u => u.Id != 1)
                    .OrderByDescending(u => u.Id)
                    .ToListAsync();
                return _mapper.Map<List<UserDTO>>(users);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting all users: {ex.Message}");
            }
        }

        public async Task<List<UserDTO>> SearchUsersAsync(string searchQuery)
        {
            try
            {
                var users = await _context.Users!
                    .Where(u => u.Id != 1 &&
                               (u.UserName.Contains(searchQuery) ||
                                u.Email.Contains(searchQuery) ||
                                (u.Name != null && u.Name.Contains(searchQuery))))
                    .OrderByDescending(u => u.Id)
                    .ToListAsync();
                return _mapper.Map<List<UserDTO>>(users);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching users: {ex.Message}");
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<int> AddUserAsync(UserDTO user)
        {
            try
            {
                var newUser = new User
                {
                    UserName = user.UserName,
                    Email = user.Email,
                };
                _context.Users!.Add(newUser);
                await _context.SaveChangesAsync();
                return newUser.Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding user: {ex.Message}");
            }
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return false;
                }

                if (user.Id == 1)
                {
                    return false;
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting user: {ex.Message}");
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}