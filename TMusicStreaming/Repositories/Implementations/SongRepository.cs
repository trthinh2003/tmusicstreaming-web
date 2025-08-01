using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using TMusicStreaming.Data;
using TMusicStreaming.DTOs.Song;
using TMusicStreaming.Helpers;
using TMusicStreaming.Models;
using TMusicStreaming.Repositories.Interfaces;

namespace TMusicStreaming.Repositories
{
    public class SongRepository : ISongRepository
    {
        private readonly TMusicStreamingContext _context;
        private readonly ILogger<SongRepository> _logger;

        public SongRepository(TMusicStreamingContext context, ILogger<SongRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<SongDTO>> GetAllSongsAsync(int? userId = null)
        {
            try
            {
                var query = _context.Songs
                    .Include(s => s.SongGenres)
                        .ThenInclude(sg => sg.Genre)
                    .Include(s => s.Album)
                    .AsQueryable();

                if (userId.HasValue)
                {
                    // Include favorites để check isFavorite
                    query = query.Include(s => s.Favorites.Where(f => f.UserId == userId.Value));
                }

                var songs = await query
                    .Select(s => new SongDTO
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        Genre = string.Join(", ", s.SongGenres.Select(sg => sg.Genre.Name)),
                        Cover = s.Cover,
                        Audio = s.SongFile,
                        Duration = s.DurationInSeconds,
                        Background = s.Image,
                        Lyric = s.LyricsFile,
                        Slug = s.Slug,
                        ReleaseDate = s.ReleaseDate ?? default(DateTime),
                        Tags = s.Tags,
                        AlbumId = s.AlbumId,
                        CreatedAt = s.CreatedAt,
                        isDisplay = s.isDisplay,
                        isLossless = s.isLossless,
                        isPopular = s.isPopular,
                        isFavorite = userId.HasValue && s.Favorites.Any(f => f.UserId == userId.Value)
                    })
                    .OrderByDescending(s => s.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();

                return songs;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<SongDTO>> GetRandomSongsReservoirAsync(int count, int? userId = null)
        {
            try
            {
                var totalCount = await _context.Songs.CountAsync();

                if (totalCount <= count)
                {
                    var allSongs = await GetAllDisplayableSongsAsync(userId);
                    return ShuffleSongs(allSongs);
                }
                return await GetRandomSongsSimpleAsync(count, userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting random songs: {ex.Message}");
            }
        }

        private async Task<List<SongDTO>> GetRandomSongsSimpleAsync(int count, int? userId = null)
        {
            var query = _context.Songs
                .Include(s => s.SongGenres)
                    .ThenInclude(sg => sg.Genre)
                .Include(s => s.Album)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Include(s => s.Favorites.Where(f => f.UserId == userId.Value));
            }

            var songs = await query
                .OrderBy(x => Guid.NewGuid())
                .Take(count)
                .Select(s => new SongDTO
                {
                    Id = s.Id,
                    Title = s.Title,
                    Artist = s.Artist,
                    Genre = string.Join(", ", s.SongGenres.Select(sg => sg.Genre.Name)),
                    Cover = s.Cover,
                    Audio = s.SongFile,
                    Duration = s.DurationInSeconds,
                    Background = s.Image,
                    Lyric = s.LyricsFile,
                    Slug = s.Slug,
                    ReleaseDate = s.ReleaseDate ?? default(DateTime),
                    Tags = s.Tags,
                    AlbumId = s.AlbumId,
                    CreatedAt = s.CreatedAt,
                    isDisplay = s.isDisplay,
                    isLossless = s.isLossless,
                    isPopular = s.isPopular,
                    isFavorite = userId.HasValue && s.Favorites.Any(f => f.UserId == userId.Value)
                })
                .AsNoTracking()
                .ToListAsync();

            return songs;
        }

        public async Task<List<SongDTO>> SearchSongAsync(string query, int? userId = null)
        {
            try
            {
                var lowerQuery = query.ToLower();
                var songQuery = _context.Songs
                    .Include(s => s.SongGenres)
                        .ThenInclude(sg => sg.Genre)
                    .Include(s => s.Album)
                    .AsQueryable();

                if (userId.HasValue)
                {
                    songQuery = songQuery.Include(s => s.Favorites.Where(f => f.UserId == userId.Value));
                }

                var songs = await songQuery
                    .Where(s => s.Title.ToLower().Contains(lowerQuery) || s.Artist.ToLower().Contains(lowerQuery))
                    .Select(s => new SongDTO
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        Genre = string.Join(", ", s.SongGenres.Select(sg => sg.Genre.Name)),
                        Cover = s.Cover,
                        Audio = s.SongFile,
                        Duration = s.DurationInSeconds,
                        Background = s.Image,
                        Lyric = s.LyricsFile,
                        Slug = s.Slug,
                        ReleaseDate = s.ReleaseDate ?? default(DateTime),
                        Tags = s.Tags,
                        AlbumId = s.AlbumId,
                        CreatedAt = s.CreatedAt,
                        isDisplay = s.isDisplay,
                        isLossless = s.isLossless,
                        isPopular = s.isPopular,
                        isFavorite = userId.HasValue && s.Favorites.Any(f => f.UserId == userId.Value)
                    })
                    .OrderByDescending(s => s.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();
                return songs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for songs.");
                throw;
            }
        }

        private async Task<List<SongDTO>> GetAllDisplayableSongsAsync(int? userId = null)
        {
            var query = _context.Songs
                .Include(s => s.SongGenres)
                    .ThenInclude(sg => sg.Genre)
                .Include(s => s.Album)
                .Where(s => s.isDisplay == true)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Include(s => s.Favorites.Where(f => f.UserId == userId.Value));
            }

            return await query
                .Select(s => new SongDTO
                {
                    Id = s.Id,
                    Title = s.Title,
                    Artist = s.Artist,
                    Genre = string.Join(", ", s.SongGenres.Select(sg => sg.Genre.Name)),
                    Cover = s.Cover,
                    Audio = s.SongFile,
                    Duration = s.DurationInSeconds,
                    Background = s.Image,
                    Lyric = s.LyricsFile,
                    Slug = s.Slug,
                    ReleaseDate = s.ReleaseDate ?? default(DateTime),
                    Tags = s.Tags,
                    AlbumId = s.AlbumId,
                    CreatedAt = s.CreatedAt,
                    isDisplay = s.isDisplay,
                    isLossless = s.isLossless,
                    isPopular = s.isPopular,
                    isFavorite = userId.HasValue && s.Favorites.Any(f => f.UserId == userId.Value)
                })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<SongDTO>> GetSongsByPlaylistIdAsync(int playlistId, int? userId = null)
        {
            try
            {
                var query = _context.Songs
                    .Where(s => s.PlaylistSongs.Any(ps => ps.PlaylistId == playlistId))
                    .Include(s => s.SongGenres)
                        .ThenInclude(sg => sg.Genre)
                    .Include(s => s.Album)
                    .AsQueryable();

                if (userId.HasValue)
                {
                    query = query.Include(s => s.Favorites.Where(f => f.UserId == userId.Value));
                }

                var songs = await query
                    .OrderByDescending(s => s.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();

                return songs.Select(s => new SongDTO
                {
                    Id = s.Id,
                    Title = s.Title,
                    Artist = s.Artist,
                    Genre = string.Join(", ", s.SongGenres.Select(sg => sg.Genre.Name)),
                    Cover = s.Cover,
                    Audio = s.SongFile,
                    Duration = s.DurationInSeconds,
                    Background = s.Image,
                    Lyric = s.LyricsFile,
                    Slug = s.Slug,
                    ReleaseDate = s.ReleaseDate ?? default,
                    Tags = s.Tags,
                    AlbumId = s.AlbumId,
                    PlaylistId = playlistId,
                    CreatedAt = s.CreatedAt,
                    isFavorite = userId.HasValue && s.Favorites.Any(f => f.UserId == userId.Value)
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bài hát theo playlistId: {PlaylistId}", playlistId);
                throw;
            }
        }

        public async Task<List<SongDTO>> GetFavoriteSongsByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting favorite songs for user {UserId}", userId);

                var favoriteSongs = await _context.Favorites
                    .Where(f => f.UserId == userId)
                    .Include(f => f.Song)
                        .ThenInclude(s => s.SongGenres)
                            .ThenInclude(sg => sg.Genre)
                    .Include(f => f.Song)
                        .ThenInclude(s => s.Album)
                    .Select(f => new SongDTO
                    {
                        Id = f.Song.Id,
                        Title = f.Song.Title,
                        Artist = f.Song.Artist,
                        Genre = string.Join(", ", f.Song.SongGenres.Select(sg => sg.Genre.Name)),
                        Cover = f.Song.Cover,
                        Audio = f.Song.SongFile,
                        Duration = f.Song.DurationInSeconds,
                        Background = f.Song.Image,
                        Lyric = f.Song.LyricsFile,
                        Slug = f.Song.Slug,
                        ReleaseDate = f.Song.ReleaseDate ?? DateTime.MinValue,
                        Tags = f.Song.Tags,
                        AlbumId = f.Song.AlbumId,
                        CreatedAt = f.Song.CreatedAt,
                        UpdatedAt = f.Song.UpdatedAt,
                        isDisplay = f.Song.isDisplay,
                        isLossless = f.Song.isLossless,
                        isPopular = f.Song.isPopular,
                        isFavorite = true,
                        playCount = _context.Histories
                            .Where(h => h.SongId == f.Song.Id && h.UserId == userId)
                            .Count()
                    })
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {Count} favorite songs for user {UserId}",
                    favoriteSongs.Count, userId);

                return favoriteSongs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorite songs for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<SongDTO>> GetSongsByArtistAsync(string artistName, int count = 5, int? userId = null)
        {
            try
            {
                var searchTerms = artistName.ToLower()
                    .Split(new[] { ' ', ',', '(', ')', '[', ']', '-', '&' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(term => term.Length > 1) // Bỏ qua các từ quá ngắn
                    .ToList();

                var query = _context.Songs
                    .Where(s => s.isDisplay == true)
                    .Include(s => s.SongGenres)
                    .ThenInclude(sg => sg.Genre)
                    .Include(s => s.Album)
                    .AsQueryable();

                // Tìm kiếm linh hoạt: bài hát match nếu artist chứa ít nhất 1 trong các từ khóa
                if (searchTerms.Any())
                {
                    query = query.Where(s => searchTerms.Any(term => s.Artist.ToLower().Contains(term)));
                }

                if (userId.HasValue)
                {
                    query = query.Include(s => s.Favorites.Where(f => f.UserId == userId.Value));
                }

                var songs = await query
                    .Take(count)
                    .Select(s => new SongDTO
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        Genre = string.Join(", ", s.SongGenres.Select(sg => sg.Genre.Name)),
                        Cover = s.Cover,
                        Audio = s.SongFile,
                        Duration = s.DurationInSeconds,
                        Background = s.Image,
                        Lyric = s.LyricsFile,
                        Slug = s.Slug,
                        ReleaseDate = s.ReleaseDate ?? default(DateTime),
                        Tags = s.Tags,
                        AlbumId = s.AlbumId,
                        CreatedAt = s.CreatedAt,
                        isDisplay = s.isDisplay,
                        isLossless = s.isLossless,
                        isPopular = s.isPopular,
                        isFavorite = userId.HasValue && s.Favorites.Any(f => f.UserId == userId.Value)
                    })
                    .OrderByDescending(s => s.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();

                return songs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bài hát theo nghệ sĩ: {ArtistName}", artistName);
                throw;
            }
        }

        public async Task<SongDTO?> GetSongByIdAsync(int id, int? userId = null)
        {
            try
            {
                var query = _context.Songs
                    .Where(s => s.Id == id)
                    .Include(s => s.SongGenres)
                        .ThenInclude(sg => sg.Genre)
                    .Include(s => s.Album)
                    .AsQueryable();

                if (userId.HasValue)
                {
                    query = query.Include(s => s.Favorites.Where(f => f.UserId == userId.Value));
                }

                var song = await query
                    .Select(s => new SongDTO
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        Genre = string.Join(", ", s.SongGenres.Select(sg => sg.Genre.Name)),
                        Cover = s.Cover,
                        Audio = s.SongFile,
                        Duration = s.DurationInSeconds,
                        Background = s.Image,
                        Lyric = s.LyricsFile,
                        Slug = s.Slug,
                        ReleaseDate = s.ReleaseDate ?? default(DateTime),
                        Tags = s.Tags,
                        AlbumId = s.AlbumId,
                        CreatedAt = s.CreatedAt,
                        isDisplay = s.isDisplay,
                        isLossless = s.isLossless,
                        isPopular = s.isPopular,
                        isFavorite = userId.HasValue && s.Favorites.Any(f => f.UserId == userId.Value)
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                return song;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while getting song by ID: {Id}", id);
                return null;
            }
        }

        public async Task<SongDTO?> GetSongBySlugAsync(string slug, int? userId = null)
        {
            try
            {
                var query = _context.Songs
                    .Where(s => s.Slug == slug)
                    .Include(s => s.SongGenres)
                        .ThenInclude(sg => sg.Genre)
                    .Include(s => s.Album)
                    .AsQueryable();

                if (userId.HasValue)
                {
                    query = query.Include(s => s.Favorites.Where(f => f.UserId == userId.Value));
                }

                var song = await query
                    .Select(s => new SongDTO
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        Genre = string.Join(", ", s.SongGenres.Select(sg => sg.Genre.Name)),
                        Cover = s.Cover,
                        Audio = s.SongFile,
                        Duration = s.DurationInSeconds,
                        Background = s.Image,
                        Lyric = s.LyricsFile,
                        Slug = s.Slug,
                        ReleaseDate = s.ReleaseDate ?? default(DateTime),
                        Tags = s.Tags,
                        AlbumId = s.AlbumId,
                        isFavorite = userId.HasValue && s.Favorites.Any(f => f.UserId == userId.Value)
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                return song;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while getting song by slug: {Slug}", slug);
                return null;
            }
        }

        public async Task<List<SongDTO>> GetPopularSongsAsync(int count = 5, int? userId = null)
        {
            try
            {
                var query = _context.Songs
                    .Where(s => s.isPopular && s.isDisplay)
                    .Include(s => s.SongGenres)
                        .ThenInclude(sg => sg.Genre)
                    .Include(s => s.Album)
                    .AsQueryable();

                if (userId.HasValue)
                {
                    query = query.Include(s => s.Favorites.Where(f => f.UserId == userId.Value));
                }

                var songs = await query
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(count)
                    .Select(s => new SongDTO
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        Genre = string.Join(", ", s.SongGenres.Select(sg => sg.Genre.Name)),
                        Cover = s.Cover,
                        Audio = s.SongFile,
                        Duration = s.DurationInSeconds,
                        Background = s.Image,
                        Lyric = s.LyricsFile,
                        Slug = s.Slug,
                        ReleaseDate = s.ReleaseDate ?? default(DateTime),
                        Tags = s.Tags,
                        AlbumId = s.AlbumId,
                        CreatedAt = s.CreatedAt,
                        isDisplay = s.isDisplay,
                        isLossless = s.isLossless,
                        isPopular = s.isPopular,
                        isFavorite = userId.HasValue && s.Favorites.Any(f => f.UserId == userId.Value),
                        playCount = new Random().Next(100000, 5000000) // Thêm dữ liệu giả cho lượt phát
                    })
                    .AsNoTracking()
                    .ToListAsync();

                return songs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular songs");
                throw;
            }
        }

        public async Task<List<SongDTO>> GetNewReleasesAsync(int count = 5, int? userId = null)
        {
            try
            {
                var query = _context.Songs
                    .Where(s => s.isDisplay)
                    .Include(s => s.SongGenres)
                        .ThenInclude(sg => sg.Genre)
                    .Include(s => s.Album)
                    .AsQueryable();

                if (userId.HasValue)
                {
                    query = query.Include(s => s.Favorites.Where(f => f.UserId == userId.Value));
                }

                var songs = await query
                    .OrderByDescending(s => s.ReleaseDate ?? s.CreatedAt)
                    .Take(count)
                    .Select(s => new SongDTO
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        Genre = string.Join(", ", s.SongGenres.Select(sg => sg.Genre.Name)),
                        Cover = s.Cover,
                        Audio = s.SongFile,
                        Duration = s.DurationInSeconds,
                        Background = s.Image,
                        Lyric = s.LyricsFile,
                        Slug = s.Slug,
                        ReleaseDate = s.ReleaseDate ?? default(DateTime),
                        Tags = s.Tags,
                        AlbumId = s.AlbumId,
                        CreatedAt = s.CreatedAt,
                        isDisplay = s.isDisplay,
                        isLossless = s.isLossless,
                        isPopular = s.isPopular,
                        isFavorite = userId.HasValue && s.Favorites.Any(f => f.UserId == userId.Value),
                        playCount = new Random().Next(10000, 1000000) // Thêm dữ liệu giả cho lượt phát
                    })
                    .AsNoTracking()
                    .ToListAsync();

                return songs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting new releases");
                throw;
            }
        }

        private List<SongDTO> ShuffleSongs(List<SongDTO> songs)
        {
            var random = new Random();
            for (int i = songs.Count - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                (songs[i], songs[j]) = (songs[j], songs[i]);
            }
            return songs;
        }

        public async Task<bool> CreateSongAsync(
            SongCreateDTO songDTO,
            List<int> genreIds,
            List<string> tags,
            string? backgroundUrl = null,
            string? coverUrl = null,
            string? songUrl = null,
            string? lyricUrl = null
)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int? albumId = null;
                if (!string.IsNullOrEmpty(songDTO.Album) && int.TryParse(songDTO.Album, out int parsedAlbumId))
                {
                    albumId = parsedAlbumId;
                }

                var newSong = new Song
                {
                    Title = songDTO.Title,
                    Artist = songDTO.Artist,
                    DurationInSeconds = songDTO.Duration,
                    ReleaseDate = songDTO.ReleaseDate.HasValue
                                ? (songDTO.ReleaseDate.Value.Kind == DateTimeKind.Utc
                                    ? songDTO.ReleaseDate.Value
                                    : songDTO.ReleaseDate.Value.ToUniversalTime())
                                : DateTime.UtcNow,
                    AlbumId = albumId, // Có thể null
                    Cover = coverUrl ?? string.Empty,
                    SongFile = songUrl ?? string.Empty,
                    LyricsFile = lyricUrl ?? string.Empty,
                    Image = backgroundUrl ?? string.Empty,
                    Slug = SlugHelper.GenerateSlug(songDTO.Title + " " + songDTO.Artist),
                    Tags = string.Join(",", tags),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Songs!.Add(newSong);
                await _context.SaveChangesAsync();

                if (genreIds != null && genreIds.Count > 0)
                {
                    var songGenres = genreIds.Select(genreId => new SongGenre
                    {
                        SongId = newSong.Id,
                        GenreId = genreId
                    });

                    _context.SongGenres!.AddRange(songGenres);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi tạo bài hát: {Message}", ex.Message);
                throw new Exception("Lỗi khi tạo bài hát: " + ex.Message);
            }
        }

        public async Task<bool> UpdateSongAsync(
            int id,
            SongCreateDTO song,
            List<int> genreIds,
            List<string> tags,
            string? backgroundUrl = null,
            string? coverUrl = null,
            string? songUrl = null,
            string? lyricUrl = null
        )
        {
            var songId = id;
            var songToUpdate = await _context.Songs!.FindAsync(songId);
            if (songToUpdate == null)
            {
                return false;
            }
            songToUpdate.Title = song.Title;
            songToUpdate.Artist = song.Artist;
            songToUpdate.DurationInSeconds = song.Duration;
            songToUpdate.ReleaseDate = song.ReleaseDate ?? DateTime.UtcNow;
            songToUpdate.AlbumId = int.Parse(song.Album);
            if (!string.IsNullOrEmpty(backgroundUrl)) songToUpdate.Image = backgroundUrl;
            if (!string.IsNullOrEmpty(coverUrl)) songToUpdate.Cover = coverUrl;
            if (!string.IsNullOrEmpty(songUrl)) songToUpdate.SongFile = songUrl;
            if (!string.IsNullOrEmpty(lyricUrl)) songToUpdate.LyricsFile = lyricUrl;
            songToUpdate.Tags = string.Join(",", tags);
            _context.Songs!.Update(songToUpdate);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSongPopularAsync(int songId, bool isPopular)
        {
            try
            {
                var song = await _context.Songs.FindAsync(songId);
                if (song == null)
                {
                    _logger.LogWarning("Song with ID {SongId} not found for popular update", songId);
                    return false;
                }

                song.isPopular = isPopular;
                song.UpdatedAt = DateTime.UtcNow;

                _context.Songs.Update(song);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Successfully updated popular status for song {SongId} to {IsPopular}", songId, isPopular);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No changes were saved when updating popular status for song {SongId}", songId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating popular status for song {SongId}", songId);
                throw new Exception($"Lỗi khi cập nhật trạng thái Popular: {ex.Message}");
            }
        }

        public Task<bool> DeleteSongAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<int> CountSongsAsync()
        {
            var songCount = await _context.Songs!.CountAsync();
            _logger.LogInformation("Song count: {Count}", songCount);
            return songCount;
        }
    }
}
