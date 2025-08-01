using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using TMusicStreaming.DTOs.Song;
using TMusicStreaming.Helpers;
using TMusicStreaming.Repositories.Implementations;
using TMusicStreaming.Repositories.Interfaces;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly ILogger<SongsController> _logger;
        private readonly ISongRepository _songRepo;
        private readonly IShareService _shareService;
        private readonly ICloudinaryService _cloudinaryService;
        public SongsController(ILogger<SongsController> logger, ISongRepository songRepo, ICloudinaryService cloudinaryService, IShareService shareService)
        {
            _logger = logger;
            _songRepo = songRepo;
            _cloudinaryService = cloudinaryService;
            _shareService = shareService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllSongsWithGenres([FromQuery] int page = 1, [FromQuery] int pageSize = 5, string query = "")
        {
            var songs = query != ""
                ? await _songRepo.SearchSongAsync(query)
                : await _songRepo.GetAllSongsAsync();
            var response = PaginationHelper.CreatePagedResponse(songs, page, pageSize);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("random-songs")]
        public async Task<IActionResult> GetRandomSongs([FromQuery] int page = 1, [FromQuery] int pageSize = 5, [FromQuery] int count = 5)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            int userId = int.Parse(userIdClaim);
            var songs = await _songRepo.GetRandomSongsReservoirAsync(count, userId);
            var response = PaginationHelper.CreatePagedResponse(songs, page, pageSize);
            return Ok(response);
        }

        [HttpGet]
        [Route("get-song-by-id/{id}")]
        public async Task<IActionResult> GetSongById(int id)
        {
            try
            {
                var song = await _songRepo.GetSongByIdAsync(id, null);
                return song == null ? NotFound() : Ok(song);
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi: {0}", ex.Message);
                throw;
            }
        }

        [Authorize]
        [HttpGet]
        [Route("{slug}")]
        public async Task<IActionResult> GetSongBySlug(string slug)
        {
            var songs = await _songRepo.GetSongBySlugAsync(slug);
            return songs == null ? NotFound() : Ok(songs);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("by-playlist/{playlistId}")]
        public async Task<IActionResult> GetSongsByPlaylist(int playlistId)
        {
            var songs = await _songRepo.GetSongsByPlaylistIdAsync(playlistId);
            var response = PaginationHelper.CreatePagedResponse(songs, 1, 30);
            return Ok(response);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavoriteSongs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("Unauthorized access attempt to favorites endpoint");
                    return Unauthorized("Người dùng chưa được xác thực");
                }

                int userId = int.Parse(userIdClaim);
                _logger.LogInformation("Getting favorite songs for user {UserId}", userId);

                var favoriteSongs = await _songRepo.GetFavoriteSongsByUserIdAsync(userId);

                if (favoriteSongs == null || favoriteSongs.Count == 0)
                {
                    return Ok(new
                    {
                        message = "Bạn chưa có bài hát yêu thích nào",
                        data = new List<object>(),
                        totalCount = 0,
                        page = page,
                        pageSize = pageSize,
                        totalPages = 0
                    });
                }

                var response = PaginationHelper.CreatePagedResponse(favoriteSongs, page, pageSize);

                _logger.LogInformation("Successfully retrieved {Count} favorite songs for user {UserId}",
                    favoriteSongs.Count, userId);

                return Ok(response);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Invalid user ID format in claims");
                return BadRequest("Định dạng ID người dùng không hợp lệ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorite songs for user");
                return StatusCode(500, new
                {
                    message = "Có lỗi xảy ra khi lấy danh sách bài hát yêu thích",
                    error = "Internal server error"
                });
            }
        }

        [Authorize]
        [HttpGet("by-artist/{artistName}")]
        public async Task<IActionResult> GetSongsByArtist(string artistName, [FromQuery] int count = 5, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int? userId = null;
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    userId = int.Parse(userIdClaim);
                }

                var songs = await _songRepo.GetSongsByArtistAsync(artistName, count, userId);

                if (songs == null || songs.Count == 0)
                {
                    return NotFound($"Không tìm thấy bài hát nào của nghệ sĩ: {artistName}");
                }

                var response = PaginationHelper.CreatePagedResponse(songs, page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy bài hát theo nghệ sĩ: {ArtistName}", artistName);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize]
        [HttpPost("share/{songId}")]
        public async Task<IActionResult> CreateShareLink(int songId, [FromQuery] int expireInMinutes = 60)
        {
            try
            {
                var song = await _songRepo.GetSongByIdAsync(songId);
                if (song == null)
                {
                    return NotFound("Bài hát không tồn tại");
                }

                var shareToken = _shareService.CreateShareLink(songId, expireInMinutes);

                return Ok(new
                {
                    shareToken = shareToken,
                    expireInMinutes = expireInMinutes,
                    song = new
                    {
                        id = song.Id,
                        title = song.Title,
                        artist = song.Artist
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating share link for song {SongId}", songId);
                return StatusCode(500, "Có lỗi xảy ra khi tạo link chia sẻ");
            }
        }


        // Endpoint lấy bài hát qua share link
        [HttpGet("shared/{shareToken}")]
        public async Task<IActionResult> GetSharedSong(string shareToken)
        {
            try
            {
                _logger.LogInformation("Received share token: {ShareToken}", shareToken);

                var (songId, isValid, expireAt, createdAt) = _shareService.ValidateShareLink(shareToken);

                if (!isValid || !songId.HasValue)
                {
                    _logger.LogWarning("Invalid or expired share token: {ShareToken}", shareToken);
                    return BadRequest(new
                    {
                        error = "Link chia sẻ không hợp lệ hoặc đã hết hạn",
                        expired = true,
                        expiredAt = expireAt?.ToString("o"),
                        createdAt = createdAt?.ToString("o")
                    });
                }

                // Gọi method với userId = null để không cần authentication
                var song = await _songRepo.GetSongByIdAsync(songId.Value, null);
                if (song == null)
                {
                    _logger.LogWarning("Song not found for id: {SongId}", songId.Value);
                    return NotFound("Bài hát không tồn tại hoặc đã bị xóa");
                }

                _logger.LogInformation("Successfully retrieved shared song: {SongId}", songId.Value);

                return Ok(new
                {
                    song = song,
                    shareInfo = new
                    {
                        expiresAt = expireAt,
                        createdAt = createdAt,
                        remainingMinutes = expireAt.HasValue ? Math.Max(0, (int)(expireAt.Value - DateTime.UtcNow).TotalMinutes) : 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shared song with token {ShareToken}", shareToken);
                return StatusCode(500, "Có lỗi xảy ra khi lấy bài hát");
            }
        }

        [Authorize]
        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularSongs([FromQuery] int count = 5)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int? userId = string.IsNullOrEmpty(userIdClaim) ? null : int.Parse(userIdClaim);

                var songs = await _songRepo.GetPopularSongsAsync(count, userId);
                return Ok(songs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular songs");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize]
        [HttpGet("new-releases")]
        public async Task<IActionResult> GetNewReleases([FromQuery] int count = 5)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int? userId = string.IsNullOrEmpty(userIdClaim) ? null : int.Parse(userIdClaim);

                var songs = await _songRepo.GetNewReleasesAsync(count, userId);
                return Ok(songs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting new releases");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] SongCreateDTO songDTO)
        {
            try
            {
                List<int> genres = new();
                List<string> tags = new();
                try
                {
                    if (!string.IsNullOrWhiteSpace(songDTO.Genres))
                        genres = JsonSerializer.Deserialize<List<int>>(songDTO.Genres) ?? new List<int>();

                    if (!string.IsNullOrWhiteSpace(songDTO.Tags))
                        tags = JsonSerializer.Deserialize<List<string>>(songDTO.Tags) ?? new List<string>();
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Lỗi khi parse Genres/Tags từ JSON.");
                    return BadRequest("Genres hoặc Tags không hợp lệ.");
                }

                var backgroundUrl = await _cloudinaryService.UploadFileAsync(songDTO.Image, "TMusicStreaming/song/images");
                var coverUrl = await _cloudinaryService.UploadFileAsync(songDTO.Cover, "TMusicStreaming/song/covers");
                var songUrl = await _cloudinaryService.UploadFileAsync(songDTO.SongFile, "TMusicStreaming/song");
                var lyricUrl = await _cloudinaryService.UploadFileAsync(songDTO.LyricsFile, "TMusicStreaming/song/lyrics");

                if (string.IsNullOrEmpty(backgroundUrl) || string.IsNullOrEmpty(coverUrl) ||
                    string.IsNullOrEmpty(songUrl) || string.IsNullOrEmpty(lyricUrl))
                {
                    return BadRequest("Tải file thất bại.");
                }
                bool isCreated = await _songRepo.CreateSongAsync(songDTO, genres ?? new List<int>(), tags ?? new List<string>()
                                                                , backgroundUrl, coverUrl, songUrl, lyricUrl);

                return isCreated
                    ? Ok(new { message = "Thêm bài hát mới thành công!" })
                    : StatusCode(500, "Thêm bài hát mới thất bại.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Có lỗi khi tạo bài hát.");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] SongCreateDTO songDTO)
        {
            try
            {
                var song = await _songRepo.GetSongByIdAsync(id);
                if (song == null)
                    return NotFound("Bài hát không tồn tại.");
                List<int> genres = new();
                List<string> tags = new();
                try
                {
                    if (!string.IsNullOrWhiteSpace(songDTO.Genres))
                        genres = JsonSerializer.Deserialize<List<int>>(songDTO.Genres) ?? new List<int>();
                    if (!string.IsNullOrWhiteSpace(songDTO.Tags))
                        tags = JsonSerializer.Deserialize<List<string>>(songDTO.Tags) ?? new List<string>();
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Lỗi khi parse Genres/Tags từ JSON.");
                    return BadRequest("Genres hoặc Tags không hợp lệ.");
                }
                string? backgroundUrl = null;
                string? coverUrl = null;
                string? songUrl = null;
                string? lyricUrl = null;
                if (songDTO.Image != null)
                    backgroundUrl = await _cloudinaryService.UploadFileAsync(songDTO.Image, "TMusicStreaming/song/images");
                if (songDTO.Cover != null)
                    coverUrl = await _cloudinaryService.UploadFileAsync(songDTO.Cover, "TMusicStreaming/song/covers");
                if (songDTO.SongFile != null)
                    songUrl = await _cloudinaryService.UploadFileAsync(songDTO.SongFile, "TMusicStreaming/song");
                if (songDTO.LyricsFile != null)
                    lyricUrl = await _cloudinaryService.UploadFileAsync(songDTO.LyricsFile, "TMusicStreaming/song/lyrics");
                bool isUpdated = await _songRepo.UpdateSongAsync(id, songDTO, genres ?? new List<int>(), tags ?? new List<string>()
                                                                , backgroundUrl, coverUrl, songUrl, lyricUrl);
                return isUpdated
                    ? Ok(new { message = "Cập nhật bài hát thành công!" })
                    : StatusCode(500, "Cập nhật bài hát thất bại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Có lỗi khi cập nhật bài hát.");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/popular")]
        public async Task<IActionResult> UpdateSongPopular(int id, [FromBody] UpdateSongPopularRequest request)
        {
            try
            {
                var song = await _songRepo.GetSongByIdAsync(id);
                if (song == null)
                {
                    return NotFound("Bài hát không tồn tại.");
                }

                bool isUpdated = await _songRepo.UpdateSongPopularAsync(id, request.IsPopular);

                if (isUpdated)
                {
                    return Ok(new
                    {
                        message = request.IsPopular ? "Đã đánh dấu bài hát là Popular!" : "Đã bỏ đánh dấu Popular!",
                        songId = id,
                        isPopular = request.IsPopular
                    });
                }
                else
                {
                    return StatusCode(500, "Cập nhật trạng thái Popular thất bại.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Có lỗi khi cập nhật trạng thái Popular cho bài hát ID: {SongId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
