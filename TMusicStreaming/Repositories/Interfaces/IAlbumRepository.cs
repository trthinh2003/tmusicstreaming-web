using TMusicStreaming.DTOs.Album;

namespace TMusicStreaming.Repositories.Interfaces
{
    public interface IAlbumRepository
    {
        Task<List<AlbumDTO>> GetAllAlbumAsync();
        Task<AlbumDTO?> GetAlbumByIdAsync(int id);
        Task<List<AlbumDTO>> GetAlbumsForCreateSong();
        Task<(List<AlbumWithSongsDTO> items, int totalCount)> GetAlbumsWithSongsPagedAsync(int page, int pageSize, string query = "");
        Task<List<AlbumDTO>> SearchAlbumAsync(string query);
        Task<bool> CreateAlbumAsync(AlbumCreateDTO album, string imageUrl);
        Task<int> CountAlbumsAsync();
        Task<bool> UpdateAlbumAsync(int id, AlbumUpdateDTO albumDto, string? imageUrl);
        Task<bool> DeleteAlbumAsync(int id);
        Task<bool> HasSongsAsync(int albumId);
    }
}
