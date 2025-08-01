using TMusicStreaming.DTOs.Genre;
using TMusicStreaming.DTOs.Paginate;

namespace TMusicStreaming.Repositories.Interfaces
{
    public interface IGenreRepository
    {
        Task<List<GenreDTO>> GetAllGenresAsync();
        Task<PagedResponse<GenreAdminDTO>> GetGenresPaginateForAdminAsync(int page, int pageSize);
        Task<GenreDTO> GetGenreAsync(int id);
        Task<List<GenreForFilterDTO>> GenreForFilterDTOAsync(string query);
        Task<bool> CreateGenreAsync(CreateGenreDTO createGenreDto);
        Task<bool> UpdateGenreAsync(int id, UpdateGenreDTO updateGenreDto);
        Task<bool> DeleteGenreAsync(int id);
    }
}
