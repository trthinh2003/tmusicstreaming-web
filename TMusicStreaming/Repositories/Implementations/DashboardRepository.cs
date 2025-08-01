using Microsoft.EntityFrameworkCore;
using TMusicStreaming.Data;
using TMusicStreaming.DTOs.Dashboard;
using TMusicStreaming.Repositories.Interfaces;
using TMusicStreaming.Models;

namespace TMusicStreaming.Repositories.Implementations
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly TMusicStreamingContext _context;
        private readonly ILogger<DashboardRepository> _logger;

        public DashboardRepository(TMusicStreamingContext context, ILogger<DashboardRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> CountAlbumsAsync() => await _context.Albums!.CountAsync();
        public async Task<int> CountArtistsAsync() => await _context.Artists!.CountAsync();
        public async Task<int> CountSongsAsync() => await _context.Songs!.CountAsync();
        public async Task<int> CountUsersAsync() => await _context.Users!.CountAsync();

        public async Task<int> CountDownloadsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Downloads.AsQueryable();
            if (startDate.HasValue) query = query.Where(d => d.DownloadDate >= startDate.Value.ToUniversalTime());
            if (endDate.HasValue) query = query.Where(d => d.DownloadDate <= endDate.Value.ToUniversalTime());
            return await query.CountAsync();
        }

        public async Task<int> CountHistoriesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Histories.AsQueryable();
            if (startDate.HasValue) query = query.Where(h => h.PlayedAt >= startDate.Value.ToUniversalTime());
            if (endDate.HasValue) query = query.Where(h => h.PlayedAt <= endDate.Value.ToUniversalTime());
            return await query.CountAsync();
        }

        public async Task<List<SongStatisticDTO>> GetTopSongsByPlaysAsync(int limit, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Histories
                .Include(h => h.Song)
                    .ThenInclude(s => s.SongGenres)
                        .ThenInclude(sg => sg.Genre)
                .Where(h => h.Song != null);

            if (startDate.HasValue) query = query.Where(h => h.PlayedAt >= startDate.Value.ToUniversalTime());
            if (endDate.HasValue) query = query.Where(h => h.PlayedAt <= endDate.Value.ToUniversalTime());

            return await query
                .GroupBy(h => h.SongId)
                .Select(g => new SongStatisticDTO
                {
                    SongId = g.Key,
                    Title = g.First().Song.Title,
                    ArtistName = g.First().Song.Artist,
                    PlayCount = g.Count(),
                    Genres = g.First().Song.SongGenres.Select(sg => sg.Genre.Name).ToList(),
                    ReleaseDate = g.First().Song.ReleaseDate ?? DateTime.MinValue
                })
                .OrderByDescending(s => s.PlayCount)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<SongStatisticDTO>> GetTopSongsByDownloadsAsync(int limit, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Downloads
                .Include(d => d.Song)
                    .ThenInclude(s => s.SongGenres)
                        .ThenInclude(sg => sg.Genre)
                .Where(d => d.Song != null);

            if (startDate.HasValue) query = query.Where(d => d.DownloadDate >= startDate.Value.ToUniversalTime());
            if (endDate.HasValue) query = query.Where(d => d.DownloadDate <= endDate.Value.ToUniversalTime());

            return await query
                .GroupBy(d => d.SongId)
                .Select(g => new SongStatisticDTO
                {
                    SongId = g.Key,
                    Title = g.First().Song.Title,
                    ArtistName = g.First().Song.Artist,
                    DownloadCount = g.Count(),
                    Genres = g.First().Song.SongGenres.Select(sg => sg.Genre.Name).ToList(),
                    ReleaseDate = g.First().Song.ReleaseDate ?? DateTime.MinValue
                })
                .OrderByDescending(s => s.DownloadCount)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<SongStatisticDTO>> GetTopSongsByFavoritesAsync(int limit, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Favorites
                .Include(f => f.Song)
                    .ThenInclude(s => s.SongGenres)
                        .ThenInclude(sg => sg.Genre)
                .Where(f => f.Song != null);

            if (startDate.HasValue) query = query.Where(f => f.Song.ReleaseDate >= startDate.Value.ToUniversalTime());
            if (endDate.HasValue) query = query.Where(f => f.Song.ReleaseDate <= endDate.Value.ToUniversalTime());

            return await query
                .GroupBy(f => f.SongId)
                .Select(g => new SongStatisticDTO
                {
                    SongId = g.Key,
                    Title = g.First().Song.Title,
                    ArtistName = g.First().Song.Artist,
                    FavoriteCount = g.Count(),
                    Genres = g.First().Song.SongGenres.Select(sg => sg.Genre.Name).ToList(),
                    ReleaseDate = g.First().Song.ReleaseDate ?? DateTime.MinValue
                })
                .OrderByDescending(s => s.FavoriteCount)
                .Take(limit)
                .ToListAsync();
        }


        public async Task<Dictionary<string, int>> GetSongCountByGenreAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.SongGenres
                .Include(sg => sg.Genre)
                .Include(sg => sg.Song)
                .AsQueryable();

            if (startDate.HasValue) query = query.Where(sg => sg.Song.CreatedAt >= startDate.Value.ToUniversalTime());
            if (endDate.HasValue) query = query.Where(sg => sg.Song.CreatedAt <= endDate.Value.ToUniversalTime());

            return await query
                .GroupBy(sg => sg.Genre.Name)
                .Select(g => new { GenreName = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.GenreName, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetNewUsersCountByPeriodAsync(string period, int count)
        {
            var now = DateTime.UtcNow;
            DateTime startDate;

            switch (period.ToLower())
            {
                case "daily":
                    startDate = now.AddDays(-count);
                    return await _context.Users
                        .Where(u => u.CreatedAt >= startDate)
                        .GroupBy(u => u.CreatedAt.Value.Date)
                        .Select(g => new
                        {
                            Period = g.Key.ToString("yyyy-MM-dd"),
                            Count = g.Count()
                        })
                        .ToDictionaryAsync(x => x.Period, x => x.Count);

                case "monthly":
                    startDate = now.AddMonths(-count);
                    return await _context.Users
                        .Where(u => u.CreatedAt >= startDate)
                        .GroupBy(u => new { u.CreatedAt.Value.Year, u.CreatedAt.Value.Month })
                        .Select(g => new
                        {
                            Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                            Count = g.Count()
                        })
                        .ToDictionaryAsync(x => x.Period, x => x.Count);

                default:
                    throw new ArgumentException("Invalid period. Use 'daily' or 'monthly'.");
            }
        }

        public async Task<Dictionary<string, int>> GetNewSongsCountByPeriodAsync(string period, int count)
        {
            var now = DateTime.UtcNow;
            DateTime startDate;

            switch (period.ToLower())
            {
                case "daily":
                    startDate = now.AddDays(-count);
                    return await _context.Songs
                        .Where(s => s.CreatedAt >= startDate)
                        .GroupBy(s => s.CreatedAt.Value.Date)
                        .Select(g => new
                        {
                            Period = g.Key.ToString("yyyy-MM-dd"),
                            Count = g.Count()
                        })
                        .ToDictionaryAsync(x => x.Period, x => x.Count);

                case "monthly":
                    startDate = now.AddMonths(-count);
                    return await _context.Songs
                        .Where(s => s.CreatedAt >= startDate)
                        .GroupBy(s => new { s.CreatedAt.Value.Year, s.CreatedAt.Value.Month })
                        .Select(g => new
                        {
                            Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                            Count = g.Count()
                        })
                        .ToDictionaryAsync(x => x.Period, x => x.Count);

                default:
                    throw new ArgumentException("Invalid period. Use 'daily' or 'monthly'.");
            }
        }

        public async Task<Dictionary<string, int>> GetTotalPlaysByPeriodAsync(string period, int count)
        {
            var now = DateTime.UtcNow;
            DateTime startDate;

            switch (period.ToLower())
            {
                case "daily":
                    startDate = now.AddDays(-count);
                    return await _context.Histories
                        .Where(h => h.PlayedAt >= startDate)
                        .GroupBy(h => h.PlayedAt.Date)
                        .Select(g => new
                        {
                            Period = g.Key.ToString("yyyy-MM-dd"),
                            Count = g.Count()
                        })
                        .ToDictionaryAsync(x => x.Period, x => x.Count);

                case "monthly":
                    startDate = now.AddMonths(-count);
                    return await _context.Histories
                        .Where(h => h.PlayedAt >= startDate)
                        .GroupBy(h => new { h.PlayedAt.Year, h.PlayedAt.Month })
                        .Select(g => new
                        {
                            Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                            Count = g.Count()
                        })
                        .ToDictionaryAsync(x => x.Period, x => x.Count);

                default:
                    throw new ArgumentException("Invalid period. Use 'daily' or 'monthly'.");
            }
        }

    }
}