using Microsoft.EntityFrameworkCore;
using TMusicStreaming.Data;
using TMusicStreaming.DTOs.Playlist;
using TMusicStreaming.DTOs.Album;
using TMusicStreaming.Models;
using TMusicStreaming.Repositories.Interfaces;

namespace TMusicStreaming.Repositories
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly TMusicStreamingContext _context;
        private readonly ILogger<PlaylistRepository> _logger;

        public PlaylistRepository(TMusicStreamingContext context, ILogger<PlaylistRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PlaylistDTO> GetPlaylistWithSongsAsync(int playlistId)
        {
            try
            {
                var playlist = await _context.Playlists
                    .Include(p => p.PlaylistSongs)
                        .ThenInclude(ps => ps.Song)
                            .ThenInclude(s => s.Album)
                    .Include(p => p.PlaylistSongs)
                        .ThenInclude(ps => ps.Song)
                            .ThenInclude(s => s.SongGenres)
                                .ThenInclude(sg => sg.Genre)
                    .FirstOrDefaultAsync(p => p.Id == playlistId);

                if (playlist == null) return null;

                return new PlaylistDTO
                {
                    Id = playlist.Id,
                    Name = playlist.Name,
                    Description = playlist.Description,
                    Image = playlist.Image,
                    UserId = playlist.UserId,
                    SongCount = playlist.PlaylistSongs.Count,
                    isDisplay = playlist.isDisplay,
                    Songs = playlist.PlaylistSongs.Select(ps => new PlaylistSongDTO
                    {
                        Id = ps.Song.Id,
                        Title = ps.Song.Title,
                        Artist = ps.Song.Artist,
                        Duration = ps.Song.DurationInSeconds,
                        Slug = ps.Song.Slug,
                        Album = ps.Song.Album != null ? new AlbumDTO
                        {
                            Id = ps.Song.Album.Id,
                            Title = ps.Song.Album.Title,
                            ImageUrl = ps.Song.Album.ImageUrl,
                            ReleaseDate = ps.Song.Album.ReleaseDate,
                            SongCount = ps.Song.Album.Songs?.Count ?? 0,
                            CreatedAt = ps.Song.Album.CreatedAt,
                            UpdatedAt = ps.Song.Album.UpdatedAt
                        } : new AlbumDTO(),
                        Genre = ps.Song.SongGenres?.Any() == true
                            ? string.Join(", ", ps.Song.SongGenres.Select(sg => sg.Genre?.Name ?? "Unknown"))
                            : "Unknown",
                        Cover = ps.Song.Cover,
                        Audio = ps.Song.SongFile,
                        Background = ps.Song.Image,
                        Lyric = ps.Song.LyricsFile
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi từ repo GetPlaylistWithSongsAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<List<PlaylistPopularDTO>> GetPlaylistsPopularAsync(int page, int pageSize, string query = "")
        {
            try
            {
                // Tạo query cơ bản
                var baseQuery = _context.Playlists
                    .Include(p => p.User)
                    .Include(p => p.PlaylistSongs)
                        .ThenInclude(ps => ps.Song)
                            .ThenInclude(s => s.Album)
                    .Include(p => p.PlaylistSongs)
                        .ThenInclude(ps => ps.Song)
                            .ThenInclude(s => s.SongGenres)
                                .ThenInclude(sg => sg.Genre)
                    .Where(p => p.isDisplay == true);

                // Áp dụng search filter nếu có
                if (!string.IsNullOrEmpty(query))
                {
                    baseQuery = baseQuery.Where(p =>
                        p.Name.Contains(query) ||
                        p.PlaylistSongs.Any(ps => ps.Song.Title.Contains(query))
                    );
                }

                // Lấy data có phân trang ngay từ database
                var playlists = await baseQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Convert sang DTO
                return playlists.Select(p => new PlaylistPopularDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Image = p.Image,
                    UserName = p.User?.UserName ?? "Unknown",
                    SongCount = p.PlaylistSongs.Count,
                    Songs = p.PlaylistSongs.Select(ps => new PlaylistSongDTO
                    {
                        Id = ps.Song.Id,
                        Title = ps.Song.Title,
                        Artist = ps.Song.Artist,
                        Duration = ps.Song.DurationInSeconds,
                        Slug = ps.Song.Slug,
                        Album = ps.Song.Album != null ? new AlbumDTO
                        {
                            Id = ps.Song.Album.Id,
                            Title = ps.Song.Album.Title,
                            ImageUrl = ps.Song.Album.ImageUrl,
                            ReleaseDate = ps.Song.Album.ReleaseDate,
                            SongCount = ps.Song.Album.Songs?.Count ?? 0,
                            CreatedAt = ps.Song.Album.CreatedAt,
                            UpdatedAt = ps.Song.Album.UpdatedAt
                        } : new AlbumDTO(),
                        Genre = ps.Song.SongGenres?.Any() == true
                            ? string.Join(", ", ps.Song.SongGenres.Select(sg => sg.Genre?.Name ?? "Unknown"))
                            : "Unknown",
                        Cover = ps.Song.Cover,
                        Audio = ps.Song.SongFile,
                        Background = ps.Song.Image,
                        Lyric = ps.Song.LyricsFile,
                    })
                    .ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi từ repo GetPlaylistsPopularAsync: {Message}", ex.Message);
                return new List<PlaylistPopularDTO>();
            }
        }

        public async Task<int> GetPlaylistsPopularCountAsync(string query = "")
        {
            try
            {
                var baseQuery = _context.Playlists
                    .Where(p => p.isDisplay == true);

                if (!string.IsNullOrEmpty(query))
                {
                    baseQuery = baseQuery.Where(p =>
                        p.Name.Contains(query) ||
                        p.PlaylistSongs.Any(ps => ps.Song.Title.Contains(query))
                    );
                }

                return await baseQuery.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi từ repo GetPlaylistsPopularCountAsync: {Message}", ex.Message);
                return 0;
            }
        }

        public async Task<IEnumerable<PlaylistDTO>> GetPlaylistsByUserAsync(int userId)
        {
            try
            {
                var playlists = await _context.Playlists
                    .Where(p => p.UserId == userId)
                    .Include(p => p.PlaylistSongs)
                        .ThenInclude(ps => ps.Song)
                            .ThenInclude(s => s.Album)
                    .Include(p => p.PlaylistSongs)
                        .ThenInclude(ps => ps.Song)
                            .ThenInclude(s => s.SongGenres)
                                .ThenInclude(sg => sg.Genre)
                    .ToListAsync();

                return playlists.Select(p => new PlaylistDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Image = p.Image,
                    UserId = p.UserId,
                    isDisplay = p.isDisplay,
                    SongCount = p.PlaylistSongs.Count,
                    Songs = p.PlaylistSongs.Select(ps => new PlaylistSongDTO
                    {
                        Id = ps.Song.Id,
                        Title = ps.Song.Title,
                        Artist = ps.Song.Artist ?? "",
                        Duration = ps.Song.DurationInSeconds,
                        Slug = ps.Song.Slug,
                        Album = ps.Song.Album != null ? new AlbumDTO
                        {
                            Id = ps.Song.Album.Id,
                            Title = ps.Song.Album.Title,
                            ImageUrl = ps.Song.Album.ImageUrl,
                            ReleaseDate = ps.Song.Album.ReleaseDate,
                            SongCount = ps.Song.Album.Songs?.Count ?? 0,
                            CreatedAt = ps.Song.Album.CreatedAt,
                            UpdatedAt = ps.Song.Album.UpdatedAt
                        } : new AlbumDTO(),
                        Genre = ps.Song.SongGenres?.Any() == true
                            ? string.Join(", ", ps.Song.SongGenres.Select(sg => sg.Genre?.Name ?? "Unknown"))
                            : "Unknown",
                        Cover = ps.Song.Cover,
                        Audio = ps.Song.SongFile,
                        Background = ps.Song.Image,
                        Lyric = ps.Song.LyricsFile
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi từ repo GetPlaylistsByUserAsync: {Message}", ex.Message);
                return new List<PlaylistDTO>();
            }
        }

        public async Task<bool> AddSongToPlaylistAsync(int playlistId, int songId)
        {
            try
            {
                var exists = await _context.PlaylistSongs
                    .AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
                if (exists) return false;

                var playlist = await _context.Playlists.FindAsync(playlistId);
                var song = await _context.Songs.FindAsync(songId);

                if (playlist != null && song != null)
                {
                    var playlistSong = new PlaylistSong { PlaylistId = playlistId, SongId = songId };
                    await _context.PlaylistSongs.AddAsync(playlistSong);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi từ repo AddSongToPlaylistAsync: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> RemoveSongFromPlaylistAsync(int playlistId, int songId)
        {
            try
            {
                var playlistSong = await _context.PlaylistSongs
                    .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
                if (playlistSong != null)
                {
                    _context.PlaylistSongs.Remove(playlistSong);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi từ repo RemoveSongFromPlaylistAsync: {Message}", ex.Message);
                return false;
            }
        }

        public async Task AddAsync(Playlist playlist)
        {
            await _context.Playlists.AddAsync(playlist);
        }

        public async Task UpdateAsync(int id, Playlist playlist)
        {
            var existingPlaylist = await _context.Playlists.FindAsync(id);
            if (existingPlaylist != null)
            {
                existingPlaylist.Name = playlist.Name;
                existingPlaylist.Description = playlist.Description;
                existingPlaylist.Image = playlist.Image;
                _context.Playlists.Update(existingPlaylist);
            }
            else
            {
                _logger.LogWarning("Playlist với ID {Id} không tìm thấy để cập nhật.", id);
                throw new KeyNotFoundException($"Playlist với ID {id} không tìm thấy");
            }
        }

        public async Task UpdatePrivacyAsync(int id, bool isDisplay)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist != null)
            {
                playlist.isDisplay = isDisplay;
                _context.Playlists.Update(playlist);
            }
            else
            {
                _logger.LogWarning("Playlist with ID {Id} not found for privacy update.", id);
                throw new KeyNotFoundException($"Playlist with ID {id} not found");
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var playlist = await _context.Playlists.FindAsync(id);
                if (playlist != null)
                {
                    _context.Playlists.Remove(playlist);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi từ repo DeleteAsync: {Message}", ex.Message);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}