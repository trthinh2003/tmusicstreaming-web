using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TMusicStreaming.Data;
using TMusicStreaming.DTOs.Song;
using TMusicStreaming.DTOs.Album;
using TMusicStreaming.DTOs.Artist;
using TMusicStreaming.Models;
using TMusicStreaming.Repositories.Interfaces;

namespace TMusicStreaming.Repositories
{
    public class AlbumRepository : IAlbumRepository
    {
        private readonly IMapper _mapper;
        private readonly TMusicStreamingContext _context;
        private readonly ILogger<AlbumRepository> _logger;
        public AlbumRepository(IMapper mapper, TMusicStreamingContext context, ILogger<AlbumRepository> logger)
        {
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        public async Task<List<AlbumDTO>> GetAllAlbumAsync()
        {
            try
            {
                var albums = await _context.Albums!
                    .Where(album => album.Id != 23)
                    .GroupJoin(
                        _context.Songs!, 
                        album => album.Id, 
                        song => song.AlbumId, 
                        (albums, songs) => new { albums, songs }
                     )
                    .Join(
                        _context.Artists!, 
                        temp => temp.albums.ArtistId, artist => artist.Id, 
                        (temp, artist) => new AlbumDTO
                        {
                            Id = temp.albums.Id,
                            Title = temp.albums.Title,
                            ImageUrl = temp.albums.ImageUrl,
                            ReleaseDate = temp.albums.ReleaseDate,
                            CreatedAt = temp.albums.CreatedAt,
                            SongCount = temp.songs.Count(),
                            Artist = _mapper.Map<ArtistDTO>(artist)
                        }
                    )
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return albums;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<AlbumDTO>> GetAlbumsForCreateSong()
        {
            try
            {
                var albums = await _context.Albums!
                    .GroupJoin(
                        _context.Songs!,
                        album => album.Id,
                        song => song.AlbumId,
                        (albums, songs) => new { albums, songs }
                     )
                    .Join(
                        _context.Artists!,
                        temp => temp.albums.ArtistId, artist => artist.Id,
                        (temp, artist) => new AlbumDTO
                        {
                            Id = temp.albums.Id,
                            Title = temp.albums.Title,
                            ImageUrl = temp.albums.ImageUrl,
                            ReleaseDate = temp.albums.ReleaseDate,
                            CreatedAt = temp.albums.CreatedAt,
                            SongCount = temp.songs.Count(),
                            Artist = _mapper.Map<ArtistDTO>(artist)
                        }
                    )
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return albums;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<(List<AlbumWithSongsDTO> items, int totalCount)> GetAlbumsWithSongsPagedAsync(int page, int pageSize, string query = "")
        {
            try
            {
                var baseQuery = _context.Albums
                    .Where(a => a.Id != 23)
                    .Include(a => a.Artist)
                    .Include(a => a.Songs)
                        .ThenInclude(s => s.SongGenres)
                            .ThenInclude(sg => sg.Genre)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(query))
                {
                    var loweredQuery = query.ToLower();
                    baseQuery = baseQuery.Where(a =>
                        a.Title.ToLower().Contains(loweredQuery) ||
                        a.Artist.Name.ToLower().Contains(loweredQuery) ||
                        a.Songs.Any(s => s.Title.ToLower().Contains(loweredQuery))
                    );
                }

                var totalCount = await baseQuery.CountAsync();

                var albums = await baseQuery
                    .OrderBy(a => a.Title)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(album => new AlbumWithSongsDTO
                    {
                        Id = album.Id,
                        Title = album.Title,
                        Artist = album.Artist.Name,
                        ImageUrl = album.ImageUrl,
                        RealeaseDate = album.ReleaseDate.Year.ToString(),
                        Songs = album.Songs.Select(song => new SongDTO
                        {
                            Id = song.Id,
                            Title = song.Title,
                            Genre = string.Join(", ", song.SongGenres.Select(sg => sg.Genre.Name)),
                            Artist = song.Artist,
                            Cover = song.Cover,
                            Audio = song.SongFile,
                            Duration = song.DurationInSeconds,
                            Background = song.Image,
                            Lyric = song.LyricsFile,
                            Slug = song.Slug,
                            ReleaseDate = song.ReleaseDate ?? DateTime.MinValue,
                            Tags = song.Tags,
                            AlbumId = song.AlbumId,
                            CreatedAt = song.CreatedAt
                        })
                        .OrderByDescending(song => song.CreatedAt)
                        .ToList()
                    })
                    .ToListAsync();

                return (albums, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged albums with songs");
                throw;
            }
        }

        public async Task<List<AlbumDTO>> SearchAlbumAsync(string query)
        {
            try
            {
                var data = await _context.Albums!
                    .GroupJoin(
                        _context.Songs!,
                        album => album.Id,
                        song => song.AlbumId,
                        (album, songs) => new { album, songs }
                    )
                    .Join(
                        _context.Artists!,
                        temp => temp.album.ArtistId,
                        artist => artist.Id,
                        (temp, artist) => new { temp.album, temp.songs, artist }
                    )
                    .Where(x => x.album.Title.Contains(query) || x.artist.Name.Contains(query))
                    .ToListAsync();

                var albums = data.Select(x => new AlbumDTO
                {
                    Id = x.album.Id,
                    Title = x.album.Title,
                    ImageUrl = x.album.ImageUrl,
                    ReleaseDate = x.album.ReleaseDate,
                    SongCount = x.songs.Count(),
                    Artist = _mapper.Map<ArtistDTO>(x.artist)
                }).ToList();

                return albums;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> CreateAlbumAsync(AlbumCreateDTO album, string? imageUrl)
        {
            try
            {
                var newAlbum = new Album
                {
                    Title = album.Title,
                    ImageUrl = imageUrl ?? string.Empty,
                    ReleaseDate = album.ReleaseDate.Kind == DateTimeKind.Utc
                                ? album.ReleaseDate
                                : album.ReleaseDate.ToUniversalTime(),
                    ArtistId = album.ArtistId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Albums!.Add(newAlbum);
                await _context.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> CountAlbumsAsync()
        {
            var albumCount = await _context.Albums!.CountAsync();
            _logger.LogInformation("Album count: {Count}", albumCount);
            return albumCount;
        }

        public async Task<AlbumDTO?> GetAlbumByIdAsync(int id)
        {
            try
            {
                var album = await _context.Albums!
                    .Include(a => a.Artist)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (album == null) return null;

                return new AlbumDTO
                {
                    Id = album.Id,
                    Title = album.Title,
                    ImageUrl = album.ImageUrl,
                    ReleaseDate = album.ReleaseDate,
                    CreatedAt = album.CreatedAt,
                    Artist = _mapper.Map<ArtistDTO>(album.Artist)
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateAlbumAsync(int id, AlbumUpdateDTO albumDto, string? imageUrl)
        {
            try
            {
                var album = await _context.Albums!.FindAsync(id);
                if (album == null) return false;

                album.Title = albumDto.Title;
                album.ArtistId = albumDto.ArtistId;
                album.ReleaseDate = albumDto.ReleaseDate.Kind == DateTimeKind.Utc
                    ? albumDto.ReleaseDate
                    : albumDto.ReleaseDate.ToUniversalTime();

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    album.ImageUrl = imageUrl;
                }

                album.UpdatedAt = DateTime.UtcNow;

                _context.Albums.Update(album);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> DeleteAlbumAsync(int id)
        {
            try
            {
                var album = await _context.Albums!.FindAsync(id);
                if (album == null) return false;

                _context.Albums.Remove(album);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> HasSongsAsync(int albumId)
        {
            try
            {
                return await _context.Songs!.AnyAsync(s => s.AlbumId == albumId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
