using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Services.Implementations
{
    public class RecommendationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly TimeSpan _interval = TimeSpan.FromHours(6); // 6 tiếng chạy 1 lần

        public RecommendationBackgroundService(IServiceProvider services)
        {
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var recommendationService = scope.ServiceProvider.GetRequiredService<IRecommendationService>();
                    //await recommendationService.CalculateUserSimilarities();
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
