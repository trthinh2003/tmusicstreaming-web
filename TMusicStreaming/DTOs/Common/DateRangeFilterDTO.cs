namespace TMusicStreaming.DTOs.Common
{
    public class DateRangeFilterDTO
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Period { get; set; } // Ví dụ: "daily", "weekly", "monthly", "custom"
    }
}
