using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMusicStreaming.DTOs.Genre;
using TMusicStreaming.Repositories.Interfaces;

namespace TMusicStreaming.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly ILogger<GenresController> _logger;
        private readonly IGenreRepository _genreRepo;

        public GenresController(ILogger<GenresController> logger, IGenreRepository genreRepo)
        {
            _logger = logger;
            _genreRepo = genreRepo;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var genres = await _genreRepo.GetAllGenresAsync();
                return Ok(genres);
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi: {0}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("paginate")]
        public async Task<IActionResult> GetGenresPaginateForAdmin([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var pagedGenres = await _genreRepo.GetGenresPaginateForAdminAsync(page, pageSize);
                return Ok(pagedGenres);
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi: {0}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGenre(int id)
        {
            try
            {
                var genre = await _genreRepo.GetGenreAsync(id);
                return Ok(genre);
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi: {0}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("for-filter")]
        public async Task<IActionResult> GetGenresForFilter([FromQuery] string? query)
        {
            try
            {
                var genres = await _genreRepo.GenreForFilterDTOAsync(query);
                return Ok(genres);
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi: {0}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateGenre([FromForm] CreateGenreDTO createGenreDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _genreRepo.CreateGenreAsync(createGenreDto);
                return Ok(new { message = "Tạo thể loại thành công", success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi: {0}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGenre(int id, [FromForm] UpdateGenreDTO updateGenreDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _genreRepo.UpdateGenreAsync(id, updateGenreDto);
                return Ok(new { message = "Cập nhật thể loại thành công", success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi: {0}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            try
            {
                var result = await _genreRepo.DeleteGenreAsync(id);
                return Ok(new { message = "Xóa thể loại thành công", success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi: {0}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
