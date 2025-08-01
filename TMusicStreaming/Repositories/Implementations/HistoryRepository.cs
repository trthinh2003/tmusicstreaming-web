using Microsoft.EntityFrameworkCore;
using System;
using TMusicStreaming.Data;
using TMusicStreaming.Models;
using TMusicStreaming.Repositories.Interfaces;

namespace TMusicStreaming.Repositories.Implementations
{
    public class HistoryRepository : IHistoryRepository
    {
        private readonly TMusicStreamingContext _context;

        public HistoryRepository(TMusicStreamingContext context)
        {
            _context = context;
        }

        public async Task<List<History>> GetUserHistoryAsync(int userId, int page = 1, int pageSize = 20)
        {
            return await _context.Histories
                .Include(h => h.Song)
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.PlayedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<History?> GetHistoryByIdAsync(int historyId)
        {
            return await _context.Histories
                .Include(h => h.Song)
                .FirstOrDefaultAsync(h => h.Id == historyId);
        }

        public async Task<bool> CreateHistoryAsync(History history)
        {
            try
            {
                _context.Histories.Add(history);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateBulkHistoryAsync(List<History> histories)
        {
            try
            {
                _context.Histories.AddRange(histories);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteHistoryAsync(int historyId)
        {
            try
            {
                var history = await _context.Histories.FindAsync(historyId);
                if (history == null) return false;

                _context.Histories.Remove(history);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<History>> GetRecentHistoryAsync(int userId, int limit = 10)
        {
            return await _context.Histories
                .Include(h => h.Song)
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.PlayedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<bool> HasUserListenedSongAsync(int userId, int songId, DateTime since)
        {
            return await _context.Histories
                .AnyAsync(h => h.UserId == userId && h.SongId == songId && h.PlayedAt >= since);
        }

        public async Task<int> GetTotalUserHistoryCountAsync(int userId)
        {
            return await _context.Histories
                .CountAsync(h => h.UserId == userId);
        }
    }
}
