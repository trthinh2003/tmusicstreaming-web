using Microsoft.EntityFrameworkCore;
using TMusicStreaming.Data;
using TMusicStreaming.DTOs.Favorite;
using TMusicStreaming.DTOs.Paginate;
using TMusicStreaming.Helpers;
using TMusicStreaming.Models;
using TMusicStreaming.Repositories.Interfaces;

namespace TMusicStreaming.Repositories.Implementations
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly TMusicStreamingContext _context;

        public FavoriteRepository(TMusicStreamingContext context)
        {
            _context = context;
        }

        public async Task<bool> AddFavoriteAsync(int userId, int songId)
        {
            try
            {
                // Kiểm tra xem đã yêu thích chưa
                var existingFavorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.SongId == songId);

                if (existingFavorite != null)
                    return false; // Đã yêu thích rồi

                // Kiểm tra user và song có tồn tại không
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                var songExists = await _context.Songs.AnyAsync(s => s.Id == songId);

                if (!userExists || !songExists)
                    return false;

                var favorite = new Favorite
                {
                    UserId = userId,
                    SongId = songId
                };

                _context.Favorites.Add(favorite);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveFavoriteAsync(int userId, int songId)
        {
            try
            {
                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.SongId == songId);

                if (favorite == null)
                    return false; // Không tìm thấy favorite để xóa

                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsFavoriteAsync(int userId, int songId)
        {
            return await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.SongId == songId);
        }

        public async Task<PagedResponse<FavoriteDTO>> GetUserFavoritesAsync(int userId, int page = 1, int pageSize = 20)
        {
            var favoritesQuery = _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Song)
                .OrderByDescending(f => f.Id)
                .Select(f => new FavoriteDTO
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    SongId = f.SongId,
                    SongTitle = f.Song.Title,
                    SongArtist = f.Song.Artist,
                    SongImage = f.Song.Cover,
                    SongSlug = f.Song.Slug,
                    Duration = f.Song.DurationInSeconds
                });

            var favorites = await favoritesQuery.ToListAsync();
            return PaginationHelper.CreatePagedResponse(favorites, page, pageSize);
        }

        public async Task<int> GetFavoriteCountAsync(int userId)
        {
            return await _context.Favorites
                .CountAsync(f => f.UserId == userId);
        }

        public async Task<List<int>> GetUserFavoriteSongIdsAsync(int userId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.SongId)
                .ToListAsync();
        }

        public async Task<Favorite?> GetFavoriteAsync(int userId, int songId)
        {
            return await _context.Favorites
                .Include(f => f.Song)
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.UserId == userId && f.SongId == songId);
        }
    }
}
