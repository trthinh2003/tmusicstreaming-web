namespace TMusicStreaming.Services.Interfaces
{
    public interface ITrackingService
    {
        Task TrackAsync(int userId, int songId, string interactionType, int score);
    }
}
