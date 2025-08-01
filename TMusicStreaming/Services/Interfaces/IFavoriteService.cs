using TMusicStreaming.DTOs.Favorite;
using TMusicStreaming.DTOs.Paginate;

namespace TMusicStreaming.Services.Interfaces
{
    public interface IFavoriteService
    {
        Task<FavoriteStatusDTO> AddToFavoriteAsync(int userId, AddFavoriteDTO addFavoriteDTO);
        Task<FavoriteStatusDTO> RemoveFromFavoriteAsync(int userId, RemoveFavoriteDTO removeFavoriteDTO);
        Task<FavoriteStatusDTO> ToggleFavoriteAsync(int userId, int songId);
        Task<bool> IsFavoriteAsync(int userId, int songId);
        Task<PagedResponse<FavoriteDTO>> GetUserFavoritesAsync(int userId, int page = 1, int pageSize = 20);
        Task<int> GetFavoriteCountAsync(int userId);
        Task<List<int>> GetUserFavoriteSongIdsAsync(int userId);
    }
}
