namespace TMusicStreaming.DTOs.Dashboard
{
    public class SongStatisticDTO
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string ArtistName { get; set; }
        public int PlayCount { get; set; }
        public int DownloadCount { get; set; }
        public int FavoriteCount { get; set; }
        public List<string> Genres { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}
