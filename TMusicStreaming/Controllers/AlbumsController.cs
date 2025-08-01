using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMusicStreaming.DTOs.Paginate;
using TMusicStreaming.DTOs.Album;
using TMusicStreaming.Helpers;
using TMusicStreaming.Repositories.Interfaces;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlbumsController : ControllerBase
    {
        private readonly IAlbumRepository _albumRepo;
        private readonly ILogger<AlbumsController> _logger;
        private readonly ICloudinaryService _cloudinaryService;

        public AlbumsController(IAlbumRepository albumRepo, ILogger<AlbumsController> logger, ICloudinaryService cloudinaryService)
        {
            _albumRepo = albumRepo;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllAlbums([FromQuery] int page = 1, [FromQuery] int pageSize = 5, string query = "")
        {
            try
            {
                var albums = query != ""
                    ? await _albumRepo.SearchAlbumAsync(query)
                    : await _albumRepo.GetAllAlbumAsync();
                var response = PaginationHelper.CreatePagedResponse(albums, page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {0}", ex.Message);
                return BadRequest(new { message = ex.Message, stack = ex.StackTrace });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAlbumById(int id)
        {
            try
            {
                var album = await _albumRepo.GetAlbumByIdAsync(id);
                if (album == null)
                    return NotFound(new { message = "Không tìm thấy album!" });

                return Ok(album);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {0}", ex.Message);
                return BadRequest(new { message = ex.Message, stack = ex.StackTrace });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-albums-for-create-song")]
        public async Task<IActionResult> GetAlbumsForCreateSong([FromQuery] int page = 1, [FromQuery] int pageSize = 5, string query = "")
        {
            try
            {
                var albums = query != ""
                    ? await _albumRepo.SearchAlbumAsync(query)
                    : await _albumRepo.GetAlbumsForCreateSong();
                var response = PaginationHelper.CreatePagedResponse(albums, page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {0}", ex.Message);
                return BadRequest(new { message = ex.Message, stack = ex.StackTrace });
            }
        }

        [Authorize]
        [HttpGet("get-albums-with-songs")]
        public async Task<IActionResult> GetAllAlbumsWithSongs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 6,
            [FromQuery] string query = ""
        )
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 6;

                var (items, totalCount) = await _albumRepo.GetAlbumsWithSongsPagedAsync(page, pageSize, query);

                var response = new PagedResponse<AlbumWithSongsDTO>
                {
                    Data = items,
                    Pagination = new PaginationInfo
                    {
                        TotalItems = totalCount,
                        CurrentPage = page,
                        PerPage = pageSize,
                        LastPage = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving albums with songs");
                return BadRequest(new { message = "An error occurred while retrieving albums" });
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] AlbumCreateDTO albumDTO)
        {
            try
            {
                string? imageUrl = null;
                if (albumDTO.Image != null)
                {
                    imageUrl = await _cloudinaryService.UploadFileAsync(albumDTO.Image, "TMusicStreaming/album/images");
                }
                bool isCreated = await _albumRepo.CreateAlbumAsync(albumDTO, imageUrl ?? string.Empty);
                return isCreated
                    ? Ok(new { message = "Thêm album mới thành công!" })
                    : BadRequest(new { message = "Thêm album mới thất bại!" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {0}", ex.Message);
                return BadRequest(new { message = ex.Message, stack = ex.StackTrace });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlbum(int id, [FromForm] AlbumUpdateDTO albumDTO)
        {
            try
            {
                var existingAlbum = await _albumRepo.GetAlbumByIdAsync(id);
                if (existingAlbum == null)
                    return NotFound(new { message = "Không tìm thấy album!" });

                string? imageUrl = existingAlbum.ImageUrl;

                // Nếu có ảnh mới được upload
                if (albumDTO.Image != null)
                {
                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(existingAlbum.ImageUrl))
                    {
                        await _cloudinaryService.DeleteFileAsync(existingAlbum.ImageUrl);
                    }

                    // Upload ảnh mới
                    imageUrl = await _cloudinaryService.UploadFileAsync(albumDTO.Image, "TMusicStreaming/album/images");
                }

                var isUpdated = await _albumRepo.UpdateAlbumAsync(id, albumDTO, imageUrl);

                return isUpdated
                    ? Ok(new { message = "Cập nhật album thành công!" })
                    : BadRequest(new { message = "Cập nhật album thất bại!" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {0}", ex.Message);
                return BadRequest(new { message = ex.Message, stack = ex.StackTrace });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlbum(int id)
        {
            try
            {
                var album = await _albumRepo.GetAlbumByIdAsync(id);
                if (album == null)
                    return NotFound(new { message = "Không tìm thấy album!" });

                // Kiểm tra xem album có bài hát nào không
                var hasSongs = await _albumRepo.HasSongsAsync(id);
                if (hasSongs)
                {
                    return BadRequest(new { message = "Không thể xóa album vì đang có bài hát!" });
                }

                // Xóa ảnh trên Cloudinary nếu có
                if (!string.IsNullOrEmpty(album.ImageUrl))
                {
                    await _cloudinaryService.DeleteFileAsync(album.ImageUrl);
                }

                var isDeleted = await _albumRepo.DeleteAlbumAsync(id);

                return isDeleted
                    ? Ok(new { message = "Xóa album thành công!" })
                    : BadRequest(new { message = "Xóa album thất bại!" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {0}", ex.Message);
                return BadRequest(new { message = ex.Message, stack = ex.StackTrace });
            }
        }
    }
}
