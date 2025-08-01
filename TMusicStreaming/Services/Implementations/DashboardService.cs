using TMusicStreaming.DTOs.Dashboard;
using TMusicStreaming.DTOs.Common;
using TMusicStreaming.Repositories.Interfaces;
using TMusicStreaming.Services.Interfaces;
using QuestPDF.Fluent;
using ClosedXML.Excel; 
using TMusicStreaming.Data;
using Microsoft.EntityFrameworkCore;

namespace TMusicStreaming.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepo;
        private readonly ICloudinaryService _cloudinaryService; 
        private readonly ILogger<DashboardService> _logger;
        private readonly TMusicStreamingContext _context;

        public DashboardService(
            IDashboardRepository dashboardRepo,
            ICloudinaryService cloudinaryService,
            TMusicStreamingContext context,
            ILogger<DashboardService> logger
        )
        {
            _dashboardRepo = dashboardRepo;
            _cloudinaryService = cloudinaryService;
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardSummaryDTO> GetOverallSummaryAsync()
        {
            var albums = await _dashboardRepo.CountAlbumsAsync();
            var artists = await _dashboardRepo.CountArtistsAsync();
            var songs = await _dashboardRepo.CountSongsAsync();
            var users = await _dashboardRepo.CountUsersAsync();
            var downloads = await _dashboardRepo.CountDownloadsAsync();
            var histories = await _dashboardRepo.CountHistoriesAsync();

            return new DashboardSummaryDTO
            {
                TotalAlbums = albums,
                TotalArtists = artists,
                TotalSongs = songs,
                TotalUsers = users,
                TotalDownloads = downloads,
                TotalPlays = histories,
                TotalComments = await _context.Comments.CountAsync()
            };
        }

        public async Task<List<SongStatisticDTO>> GetTopSongsByPlaysAsync(int limit, DateRangeFilterDTO? filter = null)
        {
            return await _dashboardRepo.GetTopSongsByPlaysAsync(limit, filter?.StartDate, filter?.EndDate);
        }

        public async Task<List<SongStatisticDTO>> GetTopSongsByDownloadsAsync(int limit, DateRangeFilterDTO? filter = null)
        {
            return await _dashboardRepo.GetTopSongsByDownloadsAsync(limit, filter?.StartDate, filter?.EndDate);
        }

        public async Task<List<SongStatisticDTO>> GetTopSongsByFavoritesAsync(int limit, DateRangeFilterDTO? filter = null)
        {
            return await _dashboardRepo.GetTopSongsByFavoritesAsync(limit, filter?.StartDate, filter?.EndDate);
        }

        public async Task<Dictionary<string, int>> GetSongCountByGenreAsync(DateRangeFilterDTO? filter = null)
        {
            return await _dashboardRepo.GetSongCountByGenreAsync(filter?.StartDate, filter?.EndDate);
        }

        public async Task<Dictionary<string, int>> GetNewUsersTrendAsync(string period = "monthly", int count = 12)
        {
            return await _dashboardRepo.GetNewUsersCountByPeriodAsync(period, count);
        }

        public async Task<Dictionary<string, int>> GetNewSongsTrendAsync(string period = "monthly", int count = 12)
        {
            return await _dashboardRepo.GetNewSongsCountByPeriodAsync(period, count);
        }

        public async Task<Dictionary<string, int>> GetTotalPlaysTrendAsync(string period = "monthly", int count = 12)
        {
            return await _dashboardRepo.GetTotalPlaysByPeriodAsync(period, count);
        }

        public async Task<byte[]> GeneratePdfReportAsync(DashboardFilterDTO filter)
        {
            var summary = await GetOverallSummaryAsync();
            var topSongsByPlays = await GetTopSongsByPlaysAsync(10, filter);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(36);
                    page.Header().Text("Music Streaming Dashboard Report")
                        .SemiBold().FontSize(24).AlignCenter();

                    page.Content().PaddingVertical(20).Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Text("Overall Summary")
                            .Bold().FontSize(18).Underline();
                        column.Item().Text($"Total Albums: {summary.TotalAlbums}");
                        column.Item().Text($"Total Artists: {summary.TotalArtists}");
                        column.Item().Text($"Total Songs: {summary.TotalSongs}");
                        column.Item().Text($"Total Users: {summary.TotalUsers}");
                        column.Item().Text($"Total Plays: {summary.TotalPlays}");
                        column.Item().Text($"Total Downloads: {summary.TotalDownloads}");

                        column.Item().Text("Top 10 Songs by Plays")
                            .Bold().FontSize(18).Underline();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Title").Bold();
                                header.Cell().Text("Artist").Bold();
                                header.Cell().Text("Plays").Bold();
                            });

                            foreach (var song in topSongsByPlays)
                            {
                                table.Cell().Text(song.Title);
                                table.Cell().Text(song.ArtistName);
                                table.Cell().Text(song.PlayCount.ToString());
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ").FontSize(10);
                        x.CurrentPageNumber().FontSize(10);
                        x.Span(" of ").FontSize(10);
                        x.TotalPages().FontSize(10);
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateExcelReportAsync(DashboardFilterDTO filter)
        {
            var summary = await GetOverallSummaryAsync();
            var topSongsByPlays = await GetTopSongsByPlaysAsync(10, filter);
            var topSongsByDownloads = await GetTopSongsByDownloadsAsync(10, filter);
            var songCountByGenre = await GetSongCountByGenreAsync(filter);
            var newUsersTrend = await GetNewUsersTrendAsync(filter.Period ?? "monthly", 12);
            var newSongsTrend = await GetNewSongsTrendAsync(filter.Period ?? "monthly", 12);
            var totalPlaysTrend = await GetTotalPlaysTrendAsync(filter.Period ?? "monthly", 12);


            using (var workbook = new XLWorkbook())
            {
                var summarySheet = workbook.Worksheets.Add("Overall Summary");
                summarySheet.Cell("A1").Value = "Statistic";
                summarySheet.Cell("B1").Value = "Value";
                summarySheet.Row(1).Style.Font.Bold = true;

                summarySheet.Cell("A2").Value = "Total Albums";
                summarySheet.Cell("B2").Value = summary.TotalAlbums;
                summarySheet.Cell("A3").Value = "Total Artists";
                summarySheet.Cell("B3").Value = summary.TotalArtists;
                summarySheet.Cell("A4").Value = "Total Songs";
                summarySheet.Cell("B4").Value = summary.TotalSongs;
                summarySheet.Cell("A5").Value = "Total Users";
                summarySheet.Cell("B5").Value = summary.TotalUsers;
                summarySheet.Cell("A6").Value = "Total Plays";
                summarySheet.Cell("B6").Value = summary.TotalPlays;
                summarySheet.Cell("A7").Value = "Total Downloads";
                summarySheet.Cell("B7").Value = summary.TotalDownloads;

                summarySheet.Columns().AdjustToContents();

                // Top Songs by Plays Sheet
                var topPlaysSheet = workbook.Worksheets.Add("Top Songs by Plays");
                topPlaysSheet.Cell("A1").Value = "Title";
                topPlaysSheet.Cell("B1").Value = "Artist";
                topPlaysSheet.Cell("C1").Value = "Plays";
                topPlaysSheet.Row(1).Style.Font.Bold = true;
                int row = 2;
                foreach (var song in topSongsByPlays)
                {
                    topPlaysSheet.Cell(row, 1).Value = song.Title;
                    topPlaysSheet.Cell(row, 2).Value = song.ArtistName;
                    topPlaysSheet.Cell(row, 3).Value = song.PlayCount;
                    row++;
                }
                topPlaysSheet.Columns().AdjustToContents();

                // Top Songs by Downloads Sheet
                var topDownloadsSheet = workbook.Worksheets.Add("Top Songs by Downloads");
                topDownloadsSheet.Cell("A1").Value = "Title";
                topDownloadsSheet.Cell("B1").Value = "Artist";
                topDownloadsSheet.Cell("C1").Value = "Downloads";
                topDownloadsSheet.Row(1).Style.Font.Bold = true;
                row = 2;
                foreach (var song in topSongsByDownloads)
                {
                    topDownloadsSheet.Cell(row, 1).Value = song.Title;
                    topDownloadsSheet.Cell(row, 2).Value = song.ArtistName;
                    topDownloadsSheet.Cell(row, 3).Value = song.DownloadCount;
                    row++;
                }
                topDownloadsSheet.Columns().AdjustToContents();

                // Đếm Số Lượng Bài Hát Theo Thể Loại
                var genreSheet = workbook.Worksheets.Add("Songs by Genre");
                genreSheet.Cell("A1").Value = "Genre";
                genreSheet.Cell("B1").Value = "Song Count";
                genreSheet.Row(1).Style.Font.Bold = true;
                row = 2;
                foreach (var entry in songCountByGenre)
                {
                    genreSheet.Cell(row, 1).Value = entry.Key;
                    genreSheet.Cell(row, 2).Value = entry.Value;
                    row++;
                }
                genreSheet.Columns().AdjustToContents();

                // New Users Trend Sheet
                var newUserTrendSheet = workbook.Worksheets.Add("New Users Trend");
                newUserTrendSheet.Cell("A1").Value = "Period";
                newUserTrendSheet.Cell("B1").Value = "New Users";
                newUserTrendSheet.Row(1).Style.Font.Bold = true;
                row = 2;
                foreach (var entry in newUsersTrend)
                {
                    newUserTrendSheet.Cell(row, 1).Value = entry.Key;
                    newUserTrendSheet.Cell(row, 2).Value = entry.Value;
                    row++;
                }
                newUserTrendSheet.Columns().AdjustToContents();

                // New Songs Trend Sheet
                var newSongTrendSheet = workbook.Worksheets.Add("New Songs Trend");
                newSongTrendSheet.Cell("A1").Value = "Period";
                newSongTrendSheet.Cell("B1").Value = "New Songs";
                newSongTrendSheet.Row(1).Style.Font.Bold = true;
                row = 2;
                foreach (var entry in newSongsTrend)
                {
                    newSongTrendSheet.Cell(row, 1).Value = entry.Key;
                    newSongTrendSheet.Cell(row, 2).Value = entry.Value;
                    row++;
                }
                newSongTrendSheet.Columns().AdjustToContents();

                var totalPlaysTrendSheet = workbook.Worksheets.Add("Total Plays Trend");
                totalPlaysTrendSheet.Cell("A1").Value = "Period";
                totalPlaysTrendSheet.Cell("B1").Value = "Total Plays";
                totalPlaysTrendSheet.Row(1).Style.Font.Bold = true;
                row = 2;
                foreach (var entry in totalPlaysTrend)
                {
                    totalPlaysTrendSheet.Cell(row, 1).Value = entry.Key;
                    totalPlaysTrendSheet.Cell(row, 2).Value = entry.Value;
                    row++;
                }
                totalPlaysTrendSheet.Columns().AdjustToContents();


                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}