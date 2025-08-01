using TMusicStreaming.DTOs.Song;

namespace TMusicStreaming.DTOs.Download
{
    public class DownloadDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SongId { get; set; }
        public DateTime DownloadDate { get; set; }
        public SongDTO Song { get; set; }
    }
}
