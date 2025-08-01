using TMusicStreaming.DTOs.Favorite;
using TMusicStreaming.DTOs.Paginate;
using TMusicStreaming.Repositories.Interfaces;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Services.Implementations
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;

        public FavoriteService(IFavoriteRepository favoriteRepository)
        {
            _favoriteRepository = favoriteRepository;
        }

        public async Task<FavoriteStatusDTO> AddToFavoriteAsync(int userId, AddFavoriteDTO addFavoriteDTO)
        {
            var result = await _favoriteRepository.AddFavoriteAsync(userId, addFavoriteDTO.SongId);

            return new FavoriteStatusDTO
            {
                IsFavorite = result,
                Message = result ? "Đã thêm vào yêu thích" : "Không thể thêm vào yêu thích hoặc đã yêu thích rồi"
            };
        }

        public async Task<FavoriteStatusDTO> RemoveFromFavoriteAsync(int userId, RemoveFavoriteDTO removeFavoriteDTO)
        {
            var result = await _favoriteRepository.RemoveFavoriteAsync(userId, removeFavoriteDTO.SongId);

            return new FavoriteStatusDTO
            {
                IsFavorite = !result,
                Message = result ? "Đã bỏ yêu thích" : "Không thể bỏ yêu thích hoặc chưa yêu thích"
            };
        }

        public async Task<FavoriteStatusDTO> ToggleFavoriteAsync(int userId, int songId)
        {
            Console.WriteLine($"[DEBUG] Toggle Favorite - UserId: {userId}, SongId: {songId}");

            var isFavorite = await _favoriteRepository.IsFavoriteAsync(userId, songId);
            Console.WriteLine($"[DEBUG] Current favorite status: {isFavorite}");

            if (isFavorite)
            {
                var removeResult = await _favoriteRepository.RemoveFavoriteAsync(userId, songId);
                var result = new FavoriteStatusDTO
                {
                    IsFavorite = !removeResult,
                    Message = removeResult ? "Đã bỏ yêu thích" : "Không thể bỏ yêu thích"
                };
                return result;
            }
            else
            {
                var addResult = await _favoriteRepository.AddFavoriteAsync(userId, songId);
                var result = new FavoriteStatusDTO
                {
                    IsFavorite = addResult,
                    Message = addResult ? "Đã thêm vào yêu thích" : "Không thể thêm vào yêu thích"
                };
                return result;
            }
        }

        public async Task<bool> IsFavoriteAsync(int userId, int songId)
        {
            return await _favoriteRepository.IsFavoriteAsync(userId, songId);
        }

        public async Task<PagedResponse<FavoriteDTO>> GetUserFavoritesAsync(int userId, int page = 1, int pageSize = 20)
        {
            return await _favoriteRepository.GetUserFavoritesAsync(userId, page, pageSize);
        }

        public async Task<int> GetFavoriteCountAsync(int userId)
        {
            return await _favoriteRepository.GetFavoriteCountAsync(userId);
        }

        public async Task<List<int>> GetUserFavoriteSongIdsAsync(int userId)
        {
            return await _favoriteRepository.GetUserFavoriteSongIdsAsync(userId);
        }
    }
}
