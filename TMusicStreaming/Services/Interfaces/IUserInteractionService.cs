using TMusicStreaming.Models;

namespace TMusicStreaming.Services.Interfaces
{
    public interface IUserInteractionService
    {
        Task RecordPlayInteractionAsync(int userId, int songId);
        Task RecordLikeInteractionAsync(int userId, int songId, bool isLiked);
        Task RecordPlaylistInteractionAsync(int userId, int songId);
        Task RecordDownloadInteractionAsync(int userId, int songId);
        Task<UserInteraction> GetOrCreateUserInteractionAsync(int userId, int songId);
        Task UpdateInteractionScoreAsync(int userId, int songId);
    }
}
