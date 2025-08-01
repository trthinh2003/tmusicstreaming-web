using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TMusicStreaming.Data;
using TMusicStreaming.DTOs.Artist;
using TMusicStreaming.DTOs.Song;
using TMusicStreaming.Models;
using TMusicStreaming.Repositories.Interfaces;

namespace TMusicStreaming.Repositories
{
    public class ArtistRepository : IArtistRepository
    {
        private readonly TMusicStreamingContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ArtistRepository> _logger;

        public ArtistRepository(TMusicStreamingContext context, IMapper mapper, ILogger<ArtistRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<ArtistDTO>> GetAllArtistAsync()
        {
            var artists = await _context.Artists!
                .Where(a => a.Id != 20)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return _mapper.Map<List<ArtistDTO>>(artists);
        }

        public async Task<ArtistDTO> GetArtistAsync(int id)
        {
            var artist = await _context.Artists!.FindAsync(id);
            return _mapper.Map<ArtistDTO>(artist);
        }

        public async Task<List<ArtistWithSongsDTO>> GetArtistsWithSongsAsync(int userId)
        {
            var followedArtists = await _context.Follows!
                .Where(f => f.UserId == userId)
                .Select(f => new { f.ArtistId, f.FollowedAt })
                .ToListAsync();
            var followedDict = followedArtists.ToDictionary(f => f.ArtistId, f => f.FollowedAt);

            // Lấy tất cả nghệ sĩ
            var artists = await _context.Artists!
                .Select(a => new ArtistWithSongsDTO
                {
                    Id = a.Id,
                    Name = a.Name,
                    Avatar = a.Avatar,
                    Bio = a.Bio,
                    DateOfBirth = a.DateOfBirth,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    IsFollowed = followedDict.ContainsKey(a.Id),
                    FollowedAt = followedDict.ContainsKey(a.Id) ? followedDict[a.Id] : null
                })
                .ToListAsync();

            // Lấy bài hát từ album
            var songsFromAlbums = await _context.Songs!
                .Where(s => s.AlbumId != null)
                .Include(s => s.Album)
                    .ThenInclude(a => a.Artist)
                .Select(s => new
                {
                    ArtistId = s.Album!.ArtistId,
                    Song = new SongForArtistDTO
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        Cover = s.Cover,
                        Audio = s.SongFile,
                        Duration = s.DurationInSeconds,
                        Lyric = s.LyricsFile,
                        ReleaseDate = s.ReleaseDate ?? DateTime.MinValue,
                        Tags = s.Tags,
                        AlbumId = s.AlbumId,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        isDisplay = s.isDisplay,
                        isLossless = s.isLossless,
                        isPopular = s.isPopular
                    }
                })
                .ToListAsync();

            // Lấy bài hát không thuộc album nào
            var songsWithoutAlbum = await _context.Songs!
                .Where(s => s.AlbumId == null)
                .ToListAsync();
            var songsWithoutAlbumMapped = artists
                .SelectMany(artist => songsWithoutAlbum
                    .Where(s => s.Artist != null &&
                           s.Artist.Split(',')
                                  .Select(part => part.Trim())
                                  .Any(part => string.Equals(part, artist.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                    .Select(s => new
                    {
                        ArtistId = artist.Id,
                        Song = new SongForArtistDTO {
                            Id = s.Id,
                            Title = s.Title,
                            Artist = s.Artist,
                            Cover = s.Cover,
                            Audio = s.SongFile,
                            Duration = s.DurationInSeconds,
                            Lyric = s.LyricsFile,
                            ReleaseDate = s.ReleaseDate ?? DateTime.MinValue,
                            Tags = s.Tags,
                            AlbumId = s.AlbumId,
                            CreatedAt = s.CreatedAt,
                            UpdatedAt = s.UpdatedAt,
                            isDisplay = s.isDisplay,
                            isLossless = s.isLossless,
                            isPopular = s.isPopular
                        }
                    }))
                .ToList();

            var allSongs = songsFromAlbums.Concat(songsWithoutAlbumMapped).ToList();
            foreach (var artist in artists)
            {
                artist.Songs = allSongs
                    .Where(s => s.ArtistId == artist.Id)
                    .Select(s => s.Song)
                    .OrderBy(s => s.Title)
                    .ToList();
            }
            // Sắp xếp: nghệ sĩ đã follow trước, sau đó theo tên
            return artists
                .OrderByDescending(a => a.IsFollowed)
                .ThenBy(a => a.Name)
                .ToList();
        }

        public async Task<ArtistWithSongsDTO?> GetArtistWithSongsAsync(int artistId, int userId)
        {
            var allArtists = await GetArtistsWithSongsAsync(userId);
            return allArtists.FirstOrDefault(a => a.Id == artistId);
        }

        public async Task<List<ArtistWithSongsDTO>> GetArtistsWithSongsAsync(int userId, int pageNumber, int pageSize)
        {
            try
            {
                var followedArtistsData = await _context.Follows!
                    .Where(f => f.UserId == userId)
                    .Select(f => new { f.ArtistId, f.FollowedAt })
                    .ToListAsync();

                var followedArtistIds = followedArtistsData.Select(f => f.ArtistId).ToHashSet();
                var followedDict = followedArtistsData.ToDictionary(f => f.ArtistId, f => f.FollowedAt);
                var totalFollowedCount = followedArtistIds.Count;
                var skipItems = (pageNumber - 1) * pageSize;

                List<ArtistWithSongsDTO> pagedArtists;

                if (skipItems < totalFollowedCount)
                {
                    // Trường hợp 1: Trang hiện tại vẫn chứa nghệ sĩ đã follow
                    var followedToTake = Math.Min(pageSize, totalFollowedCount - skipItems);
                    var unfollowedToTake = pageSize - followedToTake;

                    var followedArtists = await _context.Artists!
                        .Where(a => followedArtistIds.Contains(a.Id) && a.Id != 20) // Thêm điều kiện loại bỏ id=20
                        .OrderBy(a => a.Name)
                        .Skip(skipItems)
                        .Take(followedToTake)
                        .Select(a => new ArtistWithSongsDTO
                        {
                            Id = a.Id,
                            Name = a.Name,
                            Avatar = a.Avatar,
                            Bio = a.Bio,
                            DateOfBirth = a.DateOfBirth,
                            CreatedAt = a.CreatedAt,
                            UpdatedAt = a.UpdatedAt,
                            IsFollowed = true,
                            FollowedAt = followedDict[a.Id],
                            Songs = new List<SongForArtistDTO>()
                        })
                        .ToListAsync();

                    // Nếu còn chỗ trống, lấy thêm nghệ sĩ chưa follow
                    var unfollowedArtists = new List<ArtistWithSongsDTO>();
                    if (unfollowedToTake > 0)
                    {
                        unfollowedArtists = await _context.Artists!
                            .Where(a => !followedArtistIds.Contains(a.Id) && a.Id != 20) // Thêm điều kiện loại bỏ id=20
                            .OrderBy(a => a.Name)
                            .Take(unfollowedToTake)
                            .Select(a => new ArtistWithSongsDTO
                            {
                                Id = a.Id,
                                Name = a.Name,
                                Avatar = a.Avatar,
                                Bio = a.Bio,
                                DateOfBirth = a.DateOfBirth,
                                CreatedAt = a.CreatedAt,
                                UpdatedAt = a.UpdatedAt,
                                IsFollowed = false,
                                FollowedAt = null,
                                Songs = new List<SongForArtistDTO>()
                            })
                            .ToListAsync();
                    }

                    pagedArtists = followedArtists.Concat(unfollowedArtists).ToList();
                }
                else
                {
                    // Trường hợp 2: Trang hiện tại chỉ chứa nghệ sĩ chưa follow
                    var unfollowedSkip = skipItems - totalFollowedCount;

                    pagedArtists = await _context.Artists!
                        .Where(a => !followedArtistIds.Contains(a.Id) && a.Id != 20) // Thêm điều kiện loại bỏ id=20
                        .OrderBy(a => a.Name)
                        .Skip(unfollowedSkip)
                        .Take(pageSize)
                        .Select(a => new ArtistWithSongsDTO
                        {
                            Id = a.Id,
                            Name = a.Name,
                            Avatar = a.Avatar,
                            Bio = a.Bio,
                            DateOfBirth = a.DateOfBirth,
                            CreatedAt = a.CreatedAt,
                            UpdatedAt = a.UpdatedAt,
                            IsFollowed = false,
                            FollowedAt = null,
                            Songs = new List<SongForArtistDTO>()
                        })
                        .ToListAsync();
                }

                var artistIds = pagedArtists.Select(a => a.Id).ToList();

                var songsFromAlbums = await _context.Songs!
                    .Where(s => s.AlbumId != null && artistIds.Contains(s.Album!.ArtistId))
                    .Include(s => s.Album)
                    .Select(s => new
                    {
                        ArtistId = s.Album!.ArtistId,
                        Song = new SongForArtistDTO
                        {
                            Id = s.Id,
                            Title = s.Title,
                            Artist = s.Artist,
                            Cover = s.Cover,
                            Audio = s.SongFile,
                            Duration = s.DurationInSeconds,
                            Lyric = s.LyricsFile,
                            ReleaseDate = s.ReleaseDate ?? DateTime.MinValue,
                            Tags = s.Tags,
                            AlbumId = s.AlbumId,
                            CreatedAt = s.CreatedAt,
                            UpdatedAt = s.UpdatedAt,
                            isDisplay = s.isDisplay,
                            isLossless = s.isLossless,
                            isPopular = s.isPopular
                        }
                    })
                    .ToListAsync();

                // Tối ưu: Lấy tất cả songs without album một lần và filter trong memory
                var allSongsWithoutAlbum = await _context.Songs!
                    .Where(s => s.AlbumId == null)
                    .ToListAsync();
                var songsByArtist = songsFromAlbums.GroupBy(s => s.ArtistId).ToDictionary(g => g.Key, g => g.Select(x => x.Song).ToList());

                foreach (var artist in pagedArtists)
                {
                    var artistSongs = songsByArtist.ContainsKey(artist.Id) ? songsByArtist[artist.Id] : new List<SongForArtistDTO>();

                    // Lấy songs without album bằng cách so sánh tên
                    var songsWithoutAlbum = allSongsWithoutAlbum
                        .Where(s => string.Equals(s.Artist?.Trim(), artist.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                        .Select(s => new SongForArtistDTO
                        {
                            Id = s.Id,
                            Title = s.Title,
                            Artist = s.Artist,
                            Cover = s.Cover,
                            Audio = s.SongFile,
                            Duration = s.DurationInSeconds,
                            Lyric = s.LyricsFile,
                            ReleaseDate = s.ReleaseDate ?? DateTime.MinValue,
                            Tags = s.Tags,
                            AlbumId = s.AlbumId,
                            CreatedAt = s.CreatedAt,
                            UpdatedAt = s.UpdatedAt,
                            isDisplay = s.isDisplay,
                            isLossless = s.isLossless,
                            isPopular = s.isPopular
                        })
                        .ToList();

                    artist.Songs = artistSongs.Concat(songsWithoutAlbum)
                        .OrderBy(s => s.Title)
                        .ToList();
                }
                return pagedArtists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting artists with songs for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<ArtistWithSongsDTO>> GetArtistsWithSongsAsync(int userId, int pageNumber, int pageSize, string query = "")
        {
            try
            {
                var followedArtistsData = await _context.Follows!
                    .Where(f => f.UserId == userId)
                    .Select(f => new { f.ArtistId, f.FollowedAt })
                    .ToListAsync();

                var followedArtistIds = followedArtistsData.Select(f => f.ArtistId).ToHashSet();
                var followedDict = followedArtistsData.ToDictionary(f => f.ArtistId, f => f.FollowedAt);

                var baseQuery = _context.Artists!.Where(a => a.Id != 20);

                if (!string.IsNullOrWhiteSpace(query))
                {
                    var lowerQuery = query.ToLower().Trim();
                    baseQuery = baseQuery.Where(a =>
                        a.Name.ToLower().Contains(lowerQuery) ||
                        a.Bio.ToLower().Contains(lowerQuery) ||

                        // Có bài hát nào (trong album) có title chứa query
                        a.Albums.Any(al => al.Songs.Any(s =>
                            s.Title.ToLower().Contains(lowerQuery) ||
                            (s.Tags != null && s.Tags.ToLower().Contains(lowerQuery)) ||
                            s.Artist.ToLower().Contains(lowerQuery)
                        )) ||

                        // Hoặc có bài hát không thuộc album mà artist name trùng
                        _context.Songs
                            .Where(s => s.AlbumId == null && s.Artist != null && s.Artist.ToLower().Contains(a.Name.ToLower()))
                            .Any(s =>
                                s.Title.ToLower().Contains(lowerQuery) ||
                                (s.Tags != null && s.Tags.ToLower().Contains(lowerQuery)) ||
                                s.Artist.ToLower().Contains(lowerQuery)
                            )
                    );
                }

                // Đếm tổng số nghệ sĩ đã follow sau khi filter
                var totalFollowedAfterFilter = await baseQuery
                    .Where(a => followedArtistIds.Contains(a.Id))
                    .CountAsync();

                var skipItems = (pageNumber - 1) * pageSize;
                List<ArtistWithSongsDTO> pagedArtists;

                if (skipItems < totalFollowedAfterFilter)
                {
                    // Trường hợp 1: Trang hiện tại vẫn chứa nghệ sĩ đã follow
                    var followedToTake = Math.Min(pageSize, totalFollowedAfterFilter - skipItems);
                    var unfollowedToTake = pageSize - followedToTake;

                    var followedArtists = await baseQuery
                        .Where(a => followedArtistIds.Contains(a.Id))
                        .OrderBy(a => a.Name)
                        .Skip(skipItems)
                        .Take(followedToTake)
                        .Select(a => new ArtistWithSongsDTO
                        {
                            Id = a.Id,
                            Name = a.Name,
                            Avatar = a.Avatar,
                            Bio = a.Bio,
                            DateOfBirth = a.DateOfBirth,
                            CreatedAt = a.CreatedAt,
                            UpdatedAt = a.UpdatedAt,
                            IsFollowed = true,
                            FollowedAt = followedDict[a.Id],
                            Songs = new List<SongForArtistDTO>()
                        })
                        .ToListAsync();

                    // Nếu còn chỗ trống, lấy thêm nghệ sĩ chưa follow
                    var unfollowedArtists = new List<ArtistWithSongsDTO>();
                    if (unfollowedToTake > 0)
                    {
                        unfollowedArtists = await baseQuery
                            .Where(a => !followedArtistIds.Contains(a.Id))
                            .OrderBy(a => a.Name)
                            .Take(unfollowedToTake)
                            .Select(a => new ArtistWithSongsDTO
                            {
                                Id = a.Id,
                                Name = a.Name,
                                Avatar = a.Avatar,
                                Bio = a.Bio,
                                DateOfBirth = a.DateOfBirth,
                                CreatedAt = a.CreatedAt,
                                UpdatedAt = a.UpdatedAt,
                                IsFollowed = false,
                                FollowedAt = null,
                                Songs = new List<SongForArtistDTO>()
                            })
                            .ToListAsync();
                    }

                    pagedArtists = followedArtists.Concat(unfollowedArtists).ToList();
                }
                else
                {
                    // Trường hợp 2: Trang hiện tại chỉ chứa nghệ sĩ chưa follow
                    var unfollowedSkip = skipItems - totalFollowedAfterFilter;

                    pagedArtists = await baseQuery
                        .Where(a => !followedArtistIds.Contains(a.Id))
                        .OrderBy(a => a.Name)
                        .Skip(unfollowedSkip)
                        .Take(pageSize)
                        .Select(a => new ArtistWithSongsDTO
                        {
                            Id = a.Id,
                            Name = a.Name,
                            Avatar = a.Avatar,
                            Bio = a.Bio,
                            DateOfBirth = a.DateOfBirth,
                            CreatedAt = a.CreatedAt,
                            UpdatedAt = a.UpdatedAt,
                            IsFollowed = false,
                            FollowedAt = null,
                            Songs = new List<SongForArtistDTO>()
                        })
                        .ToListAsync();
                }

                // Load songs cho các nghệ sĩ (giữ nguyên logic cũ)
                await LoadSongsForArtists(pagedArtists, query);

                return pagedArtists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting artists with songs for user {UserId} with query {Query}", userId, query);
                throw;
            }
        }

        private async Task LoadSongsForArtists(List<ArtistWithSongsDTO> artists, string query = "")
        {
            if (!artists.Any()) return;

            var artistIds = artists.Select(a => a.Id).ToList();

            // Lấy bài hát từ album
            IQueryable<Song> songsFromAlbumsQuery = _context.Songs!
                .Where(s => s.AlbumId != null && artistIds.Contains(s.Album!.ArtistId))
                .Include(s => s.Album);

            // Thêm điều kiện tìm kiếm cho bài hát nếu có query
            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower().Trim();
                songsFromAlbumsQuery = songsFromAlbumsQuery.Where(s =>
                    s.Title.ToLower().Contains(lowerQuery) ||
                    s.Artist.ToLower().Contains(lowerQuery) ||
                    (s.Tags != null && s.Tags.ToLower().Contains(lowerQuery)));
            }

            var songsFromAlbums = await songsFromAlbumsQuery
                .Select(s => new
                {
                    ArtistId = s.Album!.ArtistId,
                    Song = new SongForArtistDTO
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        Cover = s.Cover,
                        Audio = s.SongFile,
                        Duration = s.DurationInSeconds,
                        Lyric = s.LyricsFile,
                        ReleaseDate = s.ReleaseDate ?? DateTime.MinValue,
                        Tags = s.Tags,
                        AlbumId = s.AlbumId,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        isDisplay = s.isDisplay,
                        isLossless = s.isLossless,
                        isPopular = s.isPopular
                    }
                })
                .ToListAsync();

            // Lấy bài hát không có album
            var allSongsWithoutAlbumQuery = _context.Songs!.Where(s => s.AlbumId == null);

            // Thêm điều kiện tìm kiếm cho bài hát không có album
            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower().Trim();
                allSongsWithoutAlbumQuery = allSongsWithoutAlbumQuery.Where(s =>
                    s.Title.ToLower().Contains(lowerQuery) ||
                    s.Artist.ToLower().Contains(lowerQuery) ||
                    (s.Tags != null && s.Tags.ToLower().Contains(lowerQuery)));
            }

            var allSongsWithoutAlbum = await allSongsWithoutAlbumQuery.ToListAsync();
            var songsByArtist = songsFromAlbums.GroupBy(s => s.ArtistId).ToDictionary(g => g.Key, g => g.Select(x => x.Song).ToList());

            foreach (var artist in artists)
            {
                var artistSongs = songsByArtist.ContainsKey(artist.Id) ? songsByArtist[artist.Id] : new List<SongForArtistDTO>();

                // Lấy songs without album bằng cách so sánh tên
                var songsWithoutAlbum = allSongsWithoutAlbum
                    .Where(s => !string.IsNullOrEmpty(s.Artist) &&
                                s.Artist.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Any(artistName => string.Equals(artistName.Trim(), artist.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                    .Select(s => new SongForArtistDTO
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        Cover = s.Cover,
                        Audio = s.SongFile,
                        Duration = s.DurationInSeconds,
                        Lyric = s.LyricsFile,
                        ReleaseDate = s.ReleaseDate ?? DateTime.MinValue,
                        Tags = s.Tags,
                        AlbumId = s.AlbumId,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        isDisplay = s.isDisplay,
                        isLossless = s.isLossless,
                        isPopular = s.isPopular
                    })
                    .ToList();

                artist.Songs = artistSongs.Concat(songsWithoutAlbum)
                    .OrderBy(s => s.Title)
                    .ToList();
            }
        }

        public async Task<int> GetTotalArtistsCountAsync(int userId)
        {
            return await _context.Artists!.CountAsync();
        }

        public async Task<int> GetTotalArtistsCountAsync(int userId, string query = "")
        {
            var baseQuery = _context.Artists!.Where(a => a.Id != 20);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower().Trim();
                baseQuery = baseQuery.Where(a =>
                    a.Name.ToLower().Contains(lowerQuery) ||
                    a.Bio.ToLower().Contains(lowerQuery));
            }

            return await baseQuery.CountAsync();
        }

        public async Task<List<ArtistDTO>> SearchArtistAsync(string query)
        {
            var lowerQuery = query.ToLower();
            var artists = await _context.Artists!
                .Where(a => a.Name.ToLower().Contains(lowerQuery) || a.Bio.ToLower().Contains(lowerQuery))
                .ToListAsync();
            return _mapper.Map<List<ArtistDTO>>(artists);
        }

        public async Task<bool> FollowArtistAsync(int userId, int artistId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Kiểm tra xem nghệ sĩ có tồn tại không
                var artistExists = await _context.Artists!.AnyAsync(a => a.Id == artistId);
                if (!artistExists)
                {
                    _logger.LogWarning("Artist with ID {ArtistId} not found", artistId);
                    return false;
                }

                // Kiểm tra xem đã theo dõi chưa
                var existingFollow = await _context.Follows!
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ArtistId == artistId);

                if (existingFollow != null)
                {
                    _logger.LogInformation("User {UserId} is already following artist {ArtistId}", userId, artistId);
                    return true; // Đã theo dõi rồi->trả về true liền để không hiển thị lỗi
                }

                // Tạo mối quan hệ theo dõi mới
                var follow = new Follow
                {
                    UserId = userId,
                    ArtistId = artistId,
                    FollowedAt = DateTime.UtcNow
                };

                _context.Follows!.Add(follow);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("User {UserId} successfully followed artist {ArtistId}", userId, artistId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error following artist {ArtistId} by user {UserId}", artistId, userId);
                return false;
            }
        }

        public async Task<bool> UnfollowArtistAsync(int userId, int artistId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var follow = await _context.Follows!
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ArtistId == artistId);

                if (follow == null)
                {
                    _logger.LogInformation("User {UserId} is not following artist {ArtistId}", userId, artistId);
                    return true; // Chưa theo dõi, trả về true để không hiển thị lỗi
                }

                _context.Follows!.Remove(follow);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("User {UserId} successfully unfollowed artist {ArtistId}", userId, artistId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error unfollowing artist {ArtistId} by user {UserId}", artistId, userId);
                return false;
            }
        }

        public async Task<List<ArtistWithSongsDTO>> GetFollowedArtistsAsync(int userId, int pageNumber, int pageSize)
        {
            var followedArtistIds = await _context.Follows!
                .Where(f => f.UserId == userId)
                .Select(f => f.ArtistId)
                .ToListAsync();

            if (!followedArtistIds.Any())
            {
                return new List<ArtistWithSongsDTO>();
            }

            var pagedArtists = await _context.Artists!
                .Where(a => followedArtistIds.Contains(a.Id))
                .OrderBy(a => a.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ArtistWithSongsDTO
                {
                    Id = a.Id,
                    Name = a.Name,
                    Avatar = a.Avatar,
                    Bio = a.Bio,
                    DateOfBirth = a.DateOfBirth,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    IsFollowed = true,
                    FollowedAt = _context.Follows!
                        .Where(f => f.UserId == userId && f.ArtistId == a.Id)
                        .Select(f => f.FollowedAt)
                        .FirstOrDefault(),
                    Songs = new List<SongForArtistDTO>()
                })
                .ToListAsync();

            // Load songs cho từng nghệ sĩ (tương tự logic trong GetArtistsWithSongsAsync)
            var artistIds = pagedArtists.Select(a => a.Id).ToList();

            // Lấy bài hát từ album
            var songsFromAlbums = await _context.Songs!
                .Where(s => s.AlbumId != null && artistIds.Contains(s.Album!.ArtistId))
                .Include(s => s.Album)
                .Select(s => new
                {
                    ArtistId = s.Album!.ArtistId,
                    Song = new SongForArtistDTO
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        Cover = s.Cover,
                        Audio = s.SongFile,
                        Duration = s.DurationInSeconds,
                        Lyric = s.LyricsFile,
                        ReleaseDate = s.ReleaseDate ?? DateTime.MinValue,
                        Tags = s.Tags,
                        AlbumId = s.AlbumId,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        isDisplay = s.isDisplay,
                        isLossless = s.isLossless,
                        isPopular = s.isPopular
                    }
                })
                .ToListAsync();

            // Bài hát không có album
            var songsWithoutAlbum = await _context.Songs!
                .Where(s => s.AlbumId == null)
                .ToListAsync();

            var songsWithoutAlbumMapped = pagedArtists
                .SelectMany(artist => songsWithoutAlbum
                    .Where(s => string.Equals(s.Artist.Trim(), artist.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                    .Select(s => new
                    {
                        ArtistId = artist.Id,
                        Song = new SongForArtistDTO
                        {
                            Id = s.Id,
                            Title = s.Title,
                            Artist = s.Artist,
                            Cover = s.Cover,
                            Audio = s.SongFile,
                            Duration = s.DurationInSeconds,
                            Lyric = s.LyricsFile,
                            ReleaseDate = s.ReleaseDate ?? DateTime.MinValue,
                            Tags = s.Tags,
                            AlbumId = s.AlbumId,
                            CreatedAt = s.CreatedAt,
                            UpdatedAt = s.UpdatedAt,
                            isDisplay = s.isDisplay,
                            isLossless = s.isLossless,
                            isPopular = s.isPopular
                        }
                    }))
                .ToList();

            var allSongs = songsFromAlbums.Concat(songsWithoutAlbumMapped).ToList();

            // Gán bài hát cho từng nghệ sĩ
            foreach (var artist in pagedArtists)
            {
                artist.Songs = allSongs
                    .Where(s => s.ArtistId == artist.Id)
                    .Select(s => s.Song)
                    .OrderBy(s => s.Title)
                    .ToList();
            }

            return pagedArtists;
        }

        public async Task<int> GetFollowedArtistsCountAsync(int userId)
        {
            return await _context.Follows!.CountAsync(f => f.UserId == userId);
        }

        public async Task<bool> IsFollowingArtistAsync(int userId, int artistId)
        {
            return await _context.Follows!
                .AnyAsync(f => f.UserId == userId && f.ArtistId == artistId);
        }

        public async Task<bool> CreateArtistAsync(ArtistCreateDTO artist, string? avatarUrl = null)
        {
            try
            {
                var newArtist = new Artist
                {
                    Name = artist.Name,
                    Bio = artist.Bio,
                    DateOfBirth = artist.DateOfBirth.Kind == DateTimeKind.Utc
                                ? artist.DateOfBirth
                                : artist.DateOfBirth.ToUniversalTime(),
                    Avatar = avatarUrl ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Artists!.Add(newArtist);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while creating an artist.");
                    throw;
                }
                return true;
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateArtistAsync(int id, ArtistCreateDTO dto, string? avatarUrl = null)
        {
            try
            {
                var artist = await _context.Artists!.FindAsync(id);
                if (artist == null)
                {
                    return false;
                }
                artist.Name = dto.Name ?? artist.Name;
                artist.Bio = dto.Bio ?? artist.Bio;
                artist.DateOfBirth = dto.DateOfBirth.Kind == DateTimeKind.Utc
                                ? dto.DateOfBirth
                                : dto.DateOfBirth.ToUniversalTime();
                artist.CreatedAt = artist.CreatedAt;
                artist.UpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(avatarUrl))
                {
                    artist.Avatar = avatarUrl;
                }
                _context.Artists!.Update(artist);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Task DeleteArtistAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<int> CountArtistsAsync()
        {
            var count = await _context.Artists!.CountAsync();
            _logger.LogInformation("Artist count: {Count}", count);
            return count;
        }
    }
}
