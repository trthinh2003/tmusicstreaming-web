using TMusicStreaming.DTOs.Favorite;
using TMusicStreaming.DTOs.Paginate;
using TMusicStreaming.Models;

namespace TMusicStreaming.Repositories.Interfaces
{
    public interface IFavoriteRepository
    {
        Task<bool> AddFavoriteAsync(int userId, int songId);
        Task<bool> RemoveFavoriteAsync(int userId, int songId);
        Task<bool> IsFavoriteAsync(int userId, int songId);
        Task<PagedResponse<FavoriteDTO>> GetUserFavoritesAsync(int userId, int page = 1, int pageSize = 20);
        Task<int> GetFavoriteCountAsync(int userId);
        Task<List<int>> GetUserFavoriteSongIdsAsync(int userId);
        Task<Favorite?> GetFavoriteAsync(int userId, int songId);
    }
}
