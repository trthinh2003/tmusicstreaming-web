using Microsoft.EntityFrameworkCore;
using TMusicStreaming.Data;
using TMusicStreaming.DTOs.Artist;
using TMusicStreaming.DTOs.Playlist;
using TMusicStreaming.DTOs.Song;
using TMusicStreaming.Models;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Services.Implementations
{
    public class RecommendationService : IRecommendationService
    {
        private readonly TMusicStreamingContext _context;
        private readonly ILogger<RecommendationService> _logger;

        public RecommendationService(TMusicStreamingContext context, ILogger<RecommendationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<SongDTO>> GetRecommendationsForUserAsync(int userId, int limit = 10)
        {
            try
            {
                // Lấy các bài hát user đã tương tác
                var userInteractions = await _context.UserInteractions
                    .Where(ui => ui.UserId == userId)
                    .Select(ui => ui.SongId)
                    .ToListAsync();

                // Tìm user tương tự
                var similarUsers = await _context.UserSimilarities
                    .Where(us => us.UserId1 == userId || us.UserId2 == userId)
                    .OrderByDescending(us => us.SimilarityScore)
                    .Take(5)
                    .ToListAsync();

                var recommendedSongIds = new HashSet<int>();

                // Lấy bài hát từ user tương tự
                foreach (var similarity in similarUsers)
                {
                    var otherUserId = similarity.UserId1 == userId ? similarity.UserId2 : similarity.UserId1;

                    var otherUserFavorites = await _context.UserInteractions
                        .Where(ui => ui.UserId == otherUserId && ui.InteractionScore > 5)
                        .Where(ui => !userInteractions.Contains(ui.SongId))
                        .OrderByDescending(ui => ui.InteractionScore)
                        .Take(3)
                        .Select(ui => ui.SongId)
                        .ToListAsync();

                    foreach (var songId in otherUserFavorites)
                    {
                        recommendedSongIds.Add(songId);
                    }
                }

                // Nếu không đủ, lấy thêm từ bài hát phổ biến
                if (recommendedSongIds.Count < limit)
                {
                    var popularSongs = await GetPopularSongsAsync(limit - recommendedSongIds.Count);
                    foreach (var song in popularSongs)
                    {
                        if (!userInteractions.Contains(song.Id))
                        {
                            recommendedSongIds.Add(song.Id);
                        }
                    }
                }

                //var recommendations = await _context.Songs
                //    .Where(s => recommendedSongIds.Contains(s.Id) && s.isDisplay)
                //    .Take(limit)
                //    .ToListAsync();
                var recommendations = await _context.Songs
                    .Where(s => recommendedSongIds.Contains(s.Id) && s.isDisplay)
                    .Take(limit)
                    .ToListAsync();

                return recommendations.Select(r => new SongDTO {
                    Id = r.Id,
                    Title = r.Title,
                    Artist = r.Artist,
                    Cover = r.Cover,
                    Audio = r.SongFile,
                    Background = r.Image,
                    Lyric = r.LyricsFile,
                    Tags = r.Tags,
                    playCount = 1000000
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations for user {UserId}", userId);
                return new List<SongDTO>();
            }
        }

        public async Task<List<Song>> GetSimilarSongsAsync(int songId, int limit = 10)
        {
            try
            {
                var song = await _context.Songs
                    .Include(s => s.SongGenres)
                    .FirstOrDefaultAsync(s => s.Id == songId);

                if (song == null) return new List<Song>();

                // Tìm bài hát cùng thể loại
                var genreIds = song.SongGenres.Select(sg => sg.GenreId).ToList();

                var similarSongs = await _context.Songs
                    .Where(s => s.Id != songId && s.isDisplay)
                    .Where(s => s.SongGenres.Any(sg => genreIds.Contains(sg.GenreId)))
                    .OrderByDescending(s => s.SongGenres.Count(sg => genreIds.Contains(sg.GenreId)))
                    .Take(limit)
                    .ToListAsync();

                return similarSongs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting similar songs for song {SongId}", songId);
                return new List<Song>();
            }
        }

        public async Task<List<Song>> GetPopularSongsAsync(int limit = 10)
        {
            try
            {
                var popularSongs = await _context.UserInteractions
                    .GroupBy(ui => ui.SongId)
                    .Select(g => new { SongId = g.Key, TotalScore = g.Sum(ui => ui.InteractionScore) })
                    .OrderByDescending(x => x.TotalScore)
                    .Take(limit)
                    .Join(_context.Songs,
                          x => x.SongId,
                          s => s.Id,
                          (x, s) => s)
                    .Where(s => s.isDisplay)
                    .ToListAsync();

                return popularSongs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular songs");
                return new List<Song>();
            }
        }

        public async Task<List<ArtistRecommendDTO>> GetRecommendedArtistsAsync(int userId, int limit = 10)
        {
            try
            {
                // Lấy các nghệ sĩ mà user đã follow
                var followedArtistIds = await _context.Follows
                    .Where(f => f.UserId == userId)
                    .Select(f => f.ArtistId)
                    .ToListAsync();

                // Lấy các nghệ sĩ từ bài hát user đã tương tác
                var userInteractedSongs = await _context.UserInteractions
                    .Where(ui => ui.UserId == userId)
                    .Select(ui => ui.SongId)
                    .ToListAsync();

                var artistsFromUserSongs = await _context.Songs
                    .Where(s => userInteractedSongs.Contains(s.Id))
                    .Select(s => s.Artist)
                    .Distinct()
                    .ToListAsync();

                // Tìm nghệ sĩ tương tự dựa trên genre của bài hát user thích
                var preferredGenres = await _context.Songs
                    .Where(s => userInteractedSongs.Contains(s.Id))
                    .SelectMany(s => s.SongGenres)
                    .Select(sg => sg.GenreId)
                    .Distinct()
                    .ToListAsync();

                var recommendedArtists = await _context.Artists
                    .Where(a => !followedArtistIds.Contains(a.Id) && a.Id != 20) // Không follow
                    .Where(a => a.Albums.Any(album =>
                        album.Songs.Any(song =>
                            song.SongGenres.Any(sg => preferredGenres.Contains(sg.GenreId)))))
                    .OrderByDescending(a => a.Albums.Sum(album =>
                        album.Songs.Sum(song =>
                            song.SongGenres.Count(sg => preferredGenres.Contains(sg.GenreId)))))
                    .Take(limit)
                    .ToListAsync();

                // Nếu không đủ, lấy thêm nghệ sĩ phổ biến
                if (recommendedArtists.Count < limit)
                {
                    var popularArtists = await _context.Artists
                        .Where(a => !followedArtistIds.Contains(a.Id))
                        .Where(a => !recommendedArtists.Select(ra => ra.Id).Contains(a.Id))
                        .OrderByDescending(a => a.Followers.Count())
                        .Take(limit - recommendedArtists.Count)
                        .ToListAsync();

                    recommendedArtists.AddRange(popularArtists);
                }

                return recommendedArtists.Select(a => new ArtistRecommendDTO
                {
                    Id = a.Id,
                    Name = a.Name,
                    Avatar = a.Avatar,
                    Bio = a.Bio,
                    Followers = a.Followers?.Count() ?? 0,
                    IsFollowing = false
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommended artists for user {UserId}", userId);
                return new List<ArtistRecommendDTO>();
            }
        }

        public async Task<List<PlaylistRecommendDTO>> GetRecommendedPlaylistsAsync(int userId, int limit = 10)
        {
            try
            {
                // Lấy các playlist user đã tạo
                var userPlaylistIds = await _context.Playlists
                    .Where(p => p.UserId == userId)
                    .Select(p => p.Id)
                    .ToListAsync();

                // Lấy thể loại nhạc user thích
                var userInteractedSongs = await _context.UserInteractions
                    .Where(ui => ui.UserId == userId)
                    .Select(ui => ui.SongId)
                    .ToListAsync();

                var preferredGenres = await _context.Songs
                    .Where(s => userInteractedSongs.Contains(s.Id))
                    .SelectMany(s => s.SongGenres)
                    .Select(sg => sg.GenreId)
                    .Distinct()
                    .ToListAsync();

                // Tìm playlist có nhiều bài hát cùng thể loại
                var recommendedPlaylists = await _context.Playlists
                    .Where(p => p.isDisplay && !userPlaylistIds.Contains(p.Id))
                    .Where(p => p.PlaylistSongs.Any(ps =>
                        ps.Song.SongGenres.Any(sg => preferredGenres.Contains(sg.GenreId))))
                    .OrderByDescending(p => p.PlaylistSongs.Sum(ps =>
                        ps.Song.SongGenres.Count(sg => preferredGenres.Contains(sg.GenreId))))
                    .Take(limit)
                    .Include(p => p.User)
                    .Include(p => p.PlaylistSongs)
                    .ToListAsync();

                // Nếu không đủ, lấy thêm playlist phổ biến
                if (recommendedPlaylists.Count < limit)
                {
                    var popularPlaylists = await _context.Playlists
                        .Where(p => p.isDisplay && !userPlaylistIds.Contains(p.Id))
                        .Where(p => !recommendedPlaylists.Select(rp => rp.Id).Contains(p.Id))
                        .OrderByDescending(p => p.PlaylistSongs.Count)
                        .Take(limit - recommendedPlaylists.Count)
                        .Include(p => p.User)
                        .Include(p => p.PlaylistSongs)
                        .ToListAsync();

                    recommendedPlaylists.AddRange(popularPlaylists);
                }

                return recommendedPlaylists.Select(p => new PlaylistRecommendDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description ?? "Playlist tuyệt vời",
                    Image = p.Image ?? "/src/assets/client/playlists/img/playlist_popular.jpg",
                    SongCount = p.PlaylistSongs?.Count ?? 0,
                    CreatorName = p.User?.Name ?? "Unknown"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommended playlists for user {UserId}", userId);
                return new List<PlaylistRecommendDTO>();
            }
        }

        public async Task UpdateUserSimilarityAsync(int userId)
        {
            try
            {
                // Kiểm tra user có tồn tại không
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    _logger.LogWarning("User {UserId} does not exist", userId);
                    return;
                }

                var userInteractions = await _context.UserInteractions
                    .Where(ui => ui.UserId == userId)
                    .ToDictionaryAsync(ui => ui.SongId, ui => ui.InteractionScore);

                var grouped = await _context.UserInteractions
                    .Where(ui => ui.UserId != userId)
                    .GroupBy(ui => ui.UserId)
                    .ToListAsync();

                var otherUsers = grouped
                    .Select(g => new {
                        UserId = g.Key,
                        Interactions = g.ToDictionary(ui => ui.SongId, ui => ui.InteractionScore)
                    })
                    .ToList();

                var newSimilarities = new List<UserSimilarity>();
                var updatedSimilarities = new List<UserSimilarity>();

                foreach (var otherUser in otherUsers)
                {
                    // Kiểm tra otherUser có tồn tại không
                    var otherUserExists = await _context.Users.AnyAsync(u => u.Id == otherUser.UserId);
                    if (!otherUserExists)
                    {
                        _logger.LogWarning("Other user {UserId} does not exist", otherUser.UserId);
                        continue;
                    }

                    var similarity = CalculateCosineSimilarity(userInteractions, otherUser.Interactions);

                    _logger.LogInformation(">>> Similarity between user {User1} and user {User2} = {Similarity}",
                        userId, otherUser.UserId, similarity);

                    if (similarity > 0.1)
                    {
                        var existing = await _context.UserSimilarities.FirstOrDefaultAsync(us =>
                            (us.UserId1 == userId && us.UserId2 == otherUser.UserId) ||
                            (us.UserId1 == otherUser.UserId && us.UserId2 == userId));

                        if (existing != null)
                        {
                            existing.SimilarityScore = similarity;
                            existing.LastUpdated = DateTime.UtcNow;
                            updatedSimilarities.Add(existing);
                            _logger.LogInformation(">>> Updated similarity for user {0} & {1} = {2}",
                                userId, otherUser.UserId, similarity);
                        }
                        else
                        {
                            var newSimilarity = new UserSimilarity
                            {
                                UserId1 = Math.Min(userId, otherUser.UserId),
                                UserId2 = Math.Max(userId, otherUser.UserId),
                                SimilarityScore = similarity,
                                LastUpdated = DateTime.UtcNow
                            };

                            newSimilarities.Add(newSimilarity);
                            _logger.LogInformation(">>> Added new similarity between user {0} & {1} = {2}",
                                userId, otherUser.UserId, similarity);
                        }
                    }
                }

                // Bulk add new similarities
                if (newSimilarities.Any())
                {
                    _context.UserSimilarities.AddRange(newSimilarities);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation(">>> SaveChanges for similarity update of user {0} DONE. Added: {1}, Updated: {2}",
                    userId, newSimilarities.Count, updatedSimilarities.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user similarity for user {UserId}", userId);
            }
        }

        private double CalculateCosineSimilarity(Dictionary<int, double> user1, Dictionary<int, double> user2)
        {
            var commonSongs = user1.Keys.Intersect(user2.Keys).ToList();
            if (commonSongs.Count == 0) return 0;

            double dotProduct = commonSongs.Sum(songId => user1[songId] * user2[songId]);
            double magnitude1 = Math.Sqrt(user1.Values.Sum(score => score * score));
            double magnitude2 = Math.Sqrt(user2.Values.Sum(score => score * score));

            if (magnitude1 == 0 || magnitude2 == 0) return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }
    }
}
