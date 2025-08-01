namespace TMusicStreaming.DTOs.History
{
    public class HistoryDTO
    {
        public int Id { get; set; }
        public int SongId { get; set; }
        public string SongTitle { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Cover { get; set; } = string.Empty;
        public DateTime PlayedAt { get; set; }
        public double ListenDuration { get; set; } // Thời gian nghe thực tế (s)
        public double SongDuration { get; set; } // Tổng thời gian bài hát (s)
        public double ListenPercentage { get; set; }
    }
}
