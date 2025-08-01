using TMusicStreaming.DTOs.History;
using TMusicStreaming.Models;
using TMusicStreaming.Repositories.Interfaces;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Services.Implementations
{
    public class HistoryService : IHistoryService
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly ISongRepository _songRepository;
        private readonly ILogger<HistoryService> _logger;
        private const double MINIMUM_LISTEN_PERCENTAGE = 0.7;

        public HistoryService(
            IHistoryRepository historyRepository,
            ISongRepository songRepository,
            ILogger<HistoryService> logger)
        {
            _historyRepository = historyRepository;
            _songRepository = songRepository;
            _logger = logger;
        }

        public async Task<PagedResult<HistoryDTO>> GetUserHistoryAsync(int userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var histories = await _historyRepository.GetUserHistoryAsync(userId, page, pageSize);
                var totalCount = await _historyRepository.GetTotalUserHistoryCountAsync(userId);

                var historyDTOs = histories.Select(h => new HistoryDTO
                {
                    Id = h.Id,
                    SongId = h.SongId,
                    SongTitle = h.Song?.Title ?? "",
                    Artist = h.Song?.Artist ?? "",
                    Cover = h.Song?.Cover ?? "",
                    PlayedAt = h.PlayedAt,
                    ListenDuration = ParseDuration(h.Song?.DurationInSeconds ?? "0"),
                    SongDuration = ParseDuration(h.Song?.DurationInSeconds ?? "0"),
                    ListenPercentage = 100 // Trong lịch sử, coi như đã nghe hết
                }).ToList();

                return new PagedResult<HistoryDTO>
                {
                    Data = historyDTOs,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user history for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<HistoryDTO>> GetRecentHistoryAsync(int userId, int limit = 10)
        {
            try
            {
                var histories = await _historyRepository.GetRecentHistoryAsync(userId, limit);

                return histories.Select(h => new HistoryDTO
                {
                    Id = h.Id,
                    SongId = h.SongId,
                    SongTitle = h.Song?.Title ?? "",
                    Artist = h.Song?.Artist ?? "",
                    Cover = h.Song?.Cover ?? "",
                    PlayedAt = h.PlayedAt,
                    ListenDuration = ParseDuration(h.Song?.DurationInSeconds ?? "0"),
                    SongDuration = ParseDuration(h.Song?.DurationInSeconds ?? "0"),
                    ListenPercentage = 100
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent history for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> CreateHistoryAsync(int userId, CreateHistoryDTO dto)
        {
            try
            {
                //var song = await _songRepository.GetByIdAsync(dto.SongId);
                //if (song == null)
                //{
                //    _logger.LogWarning("Song {SongId} not found when creating history", dto.SongId);
                //    return false;
                //}

                var listenPercentage = dto.SongDuration > 0 ? (dto.ListenDuration / dto.SongDuration) * 100 : 0;

                // Chỉ lưu nếu nghe >= 70-80%
                if (listenPercentage < MINIMUM_LISTEN_PERCENTAGE * 100)
                {
                    _logger.LogInformation("Song {SongId} not saved to history. Listen percentage: {Percentage}%",
                        dto.SongId, listenPercentage);
                    return true; // Trả về true vì không phải lỗi
                }

                // Kiểm tra xem đã nghe bài này trong 1 giờ qua chưa (tránh duplicate)
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);
                if (await _historyRepository.HasUserListenedSongAsync(userId, dto.SongId, oneHourAgo))
                {
                    _logger.LogInformation("Song {SongId} already in recent history for user {UserId}", dto.SongId, userId);
                    return true;
                }

                var history = new History
                {
                    UserId = userId,
                    SongId = dto.SongId,
                    PlayedAt = DateTime.UtcNow
                };

                return await _historyRepository.CreateHistoryAsync(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating history for user {UserId}, song {SongId}", userId, dto.SongId);
                return false;
            }
        }

        public async Task<bool> CreateBulkHistoryAsync(int userId, BulkCreateHistoryDTO dto)
        {
            try
            {
                var validHistories = new List<History>();
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);

                foreach (var historyDTO in dto.Histories)
                {
                    //var song = await _songRepository.GetByIdAsync(historyDTO.SongId);
                    //if (song == null) continue;

                    // Tính phần trăm nghe
                    var listenPercentage = historyDTO.SongDuration > 0 ?
                        (historyDTO.ListenDuration / historyDTO.SongDuration) * 100 : 0;

                    // Chỉ lưu nếu nghe >= 70%
                    if (listenPercentage < MINIMUM_LISTEN_PERCENTAGE * 100) continue;

                    // Kiểm tra duplicate
                    if (await _historyRepository.HasUserListenedSongAsync(userId, historyDTO.SongId, oneHourAgo))
                        continue;

                    validHistories.Add(new History
                    {
                        UserId = userId,
                        SongId = historyDTO.SongId,
                        PlayedAt = DateTime.UtcNow
                    });
                }

                if (validHistories.Any())
                {
                    return await _historyRepository.CreateBulkHistoryAsync(validHistories);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk history for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DeleteHistoryAsync(int userId, int historyId)
        {
            try
            {
                var history = await _historyRepository.GetHistoryByIdAsync(historyId);
                if (history == null || history.UserId != userId)
                {
                    return false;
                }

                return await _historyRepository.DeleteHistoryAsync(historyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting history {HistoryId} for user {UserId}", historyId, userId);
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetUserListeningStatsAsync(int userId)
        {
            try
            {
                var totalSongs = await _historyRepository.GetTotalUserHistoryCountAsync(userId);
                var recentHistory = await _historyRepository.GetRecentHistoryAsync(userId, 50);

                var stats = new Dictionary<string, object>
                {
                    ["totalSongsListened"] = totalSongs,
                    ["recentlyPlayed"] = recentHistory.Count,
                    ["favoriteArtists"] = recentHistory
                        .GroupBy(h => h.Song?.Artist)
                        .OrderByDescending(g => g.Count())
                        .Take(5)
                        .Select(g => new { Artist = g.Key, Count = g.Count() })
                        .ToList()
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting listening stats for user {UserId}", userId);
                return new Dictionary<string, object>();
            }
        }

        private double ParseDuration(string duration)
        {
            if (double.TryParse(duration, out double seconds))
            {
                return seconds;
            }
            return 0;
        }
    }
}
