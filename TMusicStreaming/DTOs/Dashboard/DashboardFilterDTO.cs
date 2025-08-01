using TMusicStreaming.DTOs.Common;

namespace TMusicStreaming.DTOs.Dashboard
{
    public class DashboardFilterDTO : DateRangeFilterDTO
    {
        public int? GenreId { get; set; }
        public int? ArtistId { get; set; }
        public int? AlbumId { get; set; }
        public bool? IsDisplay { get; set; }
        public bool? IsLossless { get; set; }
        public bool? IsPopular { get; set; }
        public string? SearchQuery { get; set; } // Cho tìm kiếm tổng quát
        public string? Role { get; set; } // Cho thống kê người dùng
    }
}