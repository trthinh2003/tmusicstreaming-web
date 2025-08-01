using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TMusicStreaming.DTOs.Artist;
using TMusicStreaming.DTOs.Paginate;
using TMusicStreaming.Helpers;
using TMusicStreaming.Models;
using TMusicStreaming.Repositories.Interfaces;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistsController : ControllerBase
    {
        private readonly IArtistRepository _artistRepo;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IArtistService _artistService;
        private readonly ILogger<ArtistsController> _logger;

        public ArtistsController(
            IArtistRepository artistRepo, 
            IArtistService artistService,
            ICloudinaryService cloudinaryService,
            ILogger<ArtistsController> logger)
        {
            _artistRepo = artistRepo;
            _artistService = artistService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }


        #region GET METHOD
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 5, string query = "")
        {
            try
            {
                var artists = query != ""
                    ? await _artistRepo.SearchArtistAsync(query)
                    : await _artistRepo.GetAllArtistAsync();

                var response = PaginationHelper.CreatePagedResponse(artists, page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize]
        [HttpGet ("{id}")]
        public async Task<IActionResult> GetArtist(int id)
        {
            var artist = await _artistRepo.GetArtistAsync(id);
            return artist == null ? NotFound() : Ok(artist);
        }

        [HttpGet("with-songs")]
        public async Task<ActionResult<List<ArtistWithSongsDTO>>> GetArtistsWithSongs(
            [FromQuery] int userId, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var artists = await _artistService.GetArtistsWithSongsAsync(userId, pageNumber, pageSize);
                return Ok(artists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("with-songs/me")]
        public async Task<ActionResult<PagedResponse<ArtistWithSongsDTO>>> GetMyArtistsWithSongs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 4,
            [FromQuery] string query = ""
        )
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
                int userId = int.Parse(userIdClaim);

                var artists = await _artistService.GetArtistsWithSongsAsync(userId, page, pageSize, query);
                var totalCount = await _artistService.GetTotalArtistsCountAsync(userId, query);

                var response = new PagedResponse<ArtistWithSongsDTO>
                {
                    Data = artists,
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{artistId}")]
        public async Task<ActionResult<ArtistWithSongsDTO>> GetArtistWithSongs(int artistId, [FromQuery] int userId)
        {
            try
            {
                var artist = await _artistService.GetArtistWithSongsAsync(artistId, userId);
                if (artist == null)
                {
                    return NotFound();
                }
                return Ok(artist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("followed")]
        public async Task<IActionResult> GetFollowedArtists([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

                int userId = int.Parse(userIdClaim);

                var followedArtists = await _artistService.GetFollowedArtistsAsync(userId, page, pageSize);
                var totalCount = await _artistService.GetFollowedArtistsCountAsync(userId);

                var response = new PagedResponse<ArtistWithSongsDTO>
                {
                    Data = followedArtists,
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
                _logger.LogError("Error getting followed artists: {0}", ex.Message);
                return StatusCode(500, "Có lỗi xảy ra khi lấy danh sách nghệ sĩ đã theo dõi.");
            }
        }
        #endregion

        #region POST METHOD
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ArtistCreateDTO artistDTO)
        {
            string? imageUrl = null;

            if (artistDTO.Avatar != null)
            {
                imageUrl = await _cloudinaryService.UploadFileAsync(artistDTO.Avatar, "TMusicStreaming/artist/images");
            }

            bool isCreated = await _artistRepo.CreateArtistAsync(artistDTO, imageUrl ?? string.Empty);
            return isCreated
                ? Ok(new { message = "Thêm nghệ sĩ mới thành công!" })
                : BadRequest("Thêm nghệ sĩ mới thất bại.");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File rỗng.");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/artists");
            Directory.CreateDirectory(uploadsFolder); // Đảm bảo thư mục tồn tại
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); // Tránh trùng tên file
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/images/artists/{fileName}"; // Đường dẫn đúng của ảnh
            return Ok(new { url = fileUrl });
        }

        [HttpPost("follow/{artistId}")]
        public async Task<IActionResult> FollowArtist(int artistId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

                int userId = int.Parse(userIdClaim);

                var result = await _artistService.FollowArtistAsync(userId, artistId);

                if (result)
                {
                    return Ok(new { message = "Đã theo dõi nghệ sĩ thành công!" });
                }

                return BadRequest("Không thể theo dõi nghệ sĩ này.");
                return Ok(new { userId, artistId });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error following artist: {0}", ex.Message);
                return StatusCode(500, "Có lỗi xảy ra khi theo dõi nghệ sĩ.");
            }
        }
        #endregion

        #region PUT/PATCH METHOD
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] ArtistCreateDTO artistDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                string? imageUrl = null;

                if (artistDTO.Avatar != null)
                {
                    imageUrl = await _cloudinaryService.UploadFileAsync(artistDTO.Avatar, "TMusicStreaming/artist/images");
                }

                bool isUpdated = await _artistRepo.UpdateArtistAsync(id, artistDTO, imageUrl);

                return isUpdated
                    ? Ok(new { message = "Cập nhật nghệ sĩ thành công!" })
                    : BadRequest("Cập nhật nghệ sĩ thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {0}", ex.Message);
                return BadRequest(new { message = ex.Message, stack = ex.StackTrace });
            }
        }
        #endregion

        #region DELETE METHOD
        [Authorize]
        [HttpDelete("unfollow/{artistId}")]
        public async Task<IActionResult> UnfollowArtist(int artistId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

                int userId = int.Parse(userIdClaim);

                var result = await _artistService.UnfollowArtistAsync(userId, artistId);

                if (result)
                {
                    return Ok(new { message = "Đã bỏ theo dõi nghệ sĩ thành công!" });
                }

                return BadRequest("Không thể bỏ theo dõi nghệ sĩ này.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error unfollowing artist: {0}", ex.Message);
                return StatusCode(500, "Có lỗi xảy ra khi bỏ theo dõi nghệ sĩ.");
            }
        }
        #endregion
    }
}
