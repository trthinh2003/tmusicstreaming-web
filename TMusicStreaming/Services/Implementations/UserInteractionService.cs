using Microsoft.EntityFrameworkCore;
using TMusicStreaming.Data;
using TMusicStreaming.Models;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Services.Implementations
{
    public class UserInteractionService : IUserInteractionService
    {
        private readonly TMusicStreamingContext _context;
        private readonly ILogger<UserInteractionService> _logger;
        private readonly IRecommendationService _recommendationService;

        public UserInteractionService(TMusicStreamingContext context, ILogger<UserInteractionService> logger, IRecommendationService recommendationService)
        {
            _context = context;
            _logger = logger;
            _recommendationService = recommendationService;
        }

        public async Task<UserInteraction> GetOrCreateUserInteractionAsync(int userId, int songId)
        {
            var interaction = await _context.UserInteractions
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.SongId == songId);

            if (interaction == null)
            {
                interaction = new UserInteraction
                {
                    UserId = userId,
                    SongId = songId,
                    PlayCount = 0,
                    IsLiked = false,
                    IsAddedToPlaylist = false,
                    IsDownloaded = false,
                    InteractionScore = 0,
                    CreatedAt = DateTime.UtcNow,
                    LastInteractedAt = DateTime.UtcNow
                };

                _context.UserInteractions.Add(interaction);
                await _context.SaveChangesAsync();
            }

            return interaction;
        }

        public async Task RecordPlayInteractionAsync(int userId, int songId)
        {
            try
            {
                var interaction = await GetOrCreateUserInteractionAsync(userId, songId);
                interaction.PlayCount++;
                interaction.LastInteractedAt = DateTime.UtcNow;

                await UpdateInteractionScoreAsync(userId, songId);
                await _recommendationService.UpdateUserSimilarityAsync(userId);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording play interaction for user {UserId}, song {SongId}", userId, songId);
            }
        }

        public async Task RecordLikeInteractionAsync(int userId, int songId, bool isLiked)
        {
            try
            {
                var interaction = await GetOrCreateUserInteractionAsync(userId, songId);
                interaction.IsLiked = isLiked;
                interaction.LastInteractedAt = DateTime.UtcNow;

                await UpdateInteractionScoreAsync(userId, songId);
                await _recommendationService.UpdateUserSimilarityAsync(userId);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording like interaction for user {UserId}, song {SongId}", userId, songId);
            }
        }

        public async Task RecordPlaylistInteractionAsync(int userId, int songId)
        {
            try
            {
                var interaction = await GetOrCreateUserInteractionAsync(userId, songId);
                interaction.IsAddedToPlaylist = true;
                interaction.LastInteractedAt = DateTime.UtcNow;

                await UpdateInteractionScoreAsync(userId, songId);
                await _recommendationService.UpdateUserSimilarityAsync(userId);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording playlist interaction for user {UserId}, song {SongId}", userId, songId);
            }
        }

        public async Task RecordDownloadInteractionAsync(int userId, int songId)
        {
            try
            {
                var interaction = await GetOrCreateUserInteractionAsync(userId, songId);
                interaction.IsDownloaded = true;
                interaction.LastInteractedAt = DateTime.UtcNow;

                await UpdateInteractionScoreAsync(userId, songId);
                await _recommendationService.UpdateUserSimilarityAsync(userId);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording download interaction for user {UserId}, song {SongId}", userId, songId);
            }
        }

        public async Task UpdateInteractionScoreAsync(int userId, int songId)
        {
            try
            {
                var interaction = await _context.UserInteractions
                    .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.SongId == songId);

                if (interaction != null)
                {
                    // Tính điểm tương tác dựa trên các hành động
                    double score = 0;
                    score += interaction.PlayCount * 0.366; // Mỗi lần nghe = 0.366 điểm
                    score += interaction.IsLiked ? 0.282 : 0; // Yêu thích = 0.282 điểm
                    score += interaction.IsAddedToPlaylist ? 0.174 : 0; // Thêm vào playlist = 0.174 điểm
                    score += interaction.IsDownloaded ? 0.197 : 0; // Download = 0.197 điểm

                    interaction.InteractionScore = score;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating interaction score for user {UserId}, song {SongId}", userId, songId);
            }
        }
    }
}
