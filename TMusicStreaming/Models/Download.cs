namespace TMusicStreaming.Models
{
    public class Download
    {
        public int Id { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
        public Song Song { get; set; }
        public int SongId { get; set; }
        public DateTime DownloadDate { get; set; }
    }
}
