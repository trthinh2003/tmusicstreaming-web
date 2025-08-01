using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Services.Implementations
{
    public class TrackingService : ITrackingService
    {
        private readonly ILogger<TrackingService> _logger;

        public TrackingService(ILogger<TrackingService> logger)
        {
            _logger = logger;
        }

        public Task TrackAsync(int userId, int songId, string interactionType, int score)
        {
            throw new NotImplementedException();
        }
    }
}
