using Microsoft.EntityFrameworkCore;
using TMusicStreaming.Data;
using TMusicStreaming.Models;
using TMusicStreaming.Repositories.Interfaces;

namespace TMusicStreaming.Repositories.Implementations
{
    public class DownloadRepository : IDownloadRepository
    {
        private readonly TMusicStreamingContext _context;

        public DownloadRepository(TMusicStreamingContext context)
        {
            _context = context;
        }

        public async Task<Download> CreateDownloadAsync(int userId, int songId)
        {
            var download = new Download
            {
                UserId = userId,
                SongId = songId,
                DownloadDate = DateTime.UtcNow
            };

            _context.Downloads.Add(download);
            await _context.SaveChangesAsync();

            return await _context.Downloads
                .Include(d => d.Song)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == download.Id);
        }

        public async Task<List<Download>> GetUserDownloadsAsync(int userId, int page = 1, int pageSize = 20)
        {
            return await _context.Downloads
                .Where(d => d.UserId == userId)
                .Include(d => d.Song)
                .OrderByDescending(d => d.DownloadDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> HasDownloadedAsync(int userId, int songId)
        {
            return await _context.Downloads
                .AnyAsync(d => d.UserId == userId && d.SongId == songId);
        }

        public async Task<int> GetDownloadCountBySongAsync(int songId)
        {
            return await _context.Downloads
                .CountAsync(d => d.SongId == songId);
        }

        public async Task<List<Download>> GetRecentDownloadsAsync(int userId, int limit = 10)
        {
            return await _context.Downloads
                .Where(d => d.UserId == userId)
                .Include(d => d.Song)
                .OrderByDescending(d => d.DownloadDate)
                .Take(limit)
                .ToListAsync();
        }
    }
}
