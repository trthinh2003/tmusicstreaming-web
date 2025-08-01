using TMusicStreaming.DTOs.Download;
using TMusicStreaming.DTOs.Song;
using TMusicStreaming.Repositories.Interfaces;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Services.Implementations
{
    public class DownloadService : IDownloadService
    {
        private readonly IDownloadRepository _downloadRepository;

        public DownloadService(IDownloadRepository downloadRepository)
        {
            _downloadRepository = downloadRepository;
        }

        public async Task<DownloadDTO> RecordDownloadAsync(int userId, int songId)
        {
            var download = await _downloadRepository.CreateDownloadAsync(userId, songId);

            return new DownloadDTO
            {
                Id = download.Id,
                UserId = download.UserId,
                SongId = download.SongId,
                DownloadDate = download.DownloadDate,
                Song = new SongDTO
                {
                    Id = download.Song.Id,
                    Title = download.Song.Title,
                    Artist = download.Song.Artist,
                    Cover = download.Song.Cover,
                    Audio = download.Song.SongFile
                }
            };
        }

        public async Task<List<DownloadDTO>> GetUserDownloadsAsync(int userId, int page = 1, int pageSize = 20)
        {
            var downloads = await _downloadRepository.GetUserDownloadsAsync(userId, page, pageSize);

            return downloads.Select(d => new DownloadDTO
            {
                Id = d.Id,
                UserId = d.UserId,
                SongId = d.SongId,
                DownloadDate = d.DownloadDate,
                Song = new SongDTO
                {
                    Id = d.Song.Id,
                    Title = d.Song.Title,
                    Artist = d.Song.Artist,
                    Cover = d.Song.Cover,
                    Audio = d.Song.SongFile
                }
            }).ToList();
        }

        public async Task<bool> HasUserDownloadedSongAsync(int userId, int songId)
        {
            return await _downloadRepository.HasDownloadedAsync(userId, songId);
        }

        public async Task<DownloadStatsDTO> GetDownloadStatsAsync(int songId)
        {
            var count = await _downloadRepository.GetDownloadCountBySongAsync(songId);

            return new DownloadStatsDTO
            {
                SongId = songId,
                TotalDownloads = count
            };
        }
    }
}
