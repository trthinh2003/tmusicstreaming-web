namespace TMusicStreaming.DTOs.History
{
    public class CreateHistoryDTO
    {
        public int SongId { get; set; }
        public double ListenDuration { get; set; } 
        public double SongDuration { get; set; }
    }
}
