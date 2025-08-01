using Microsoft.EntityFrameworkCore;
using TMusicStreaming.DTOs.Song;
using TMusicStreaming.Models;

namespace TMusicStreaming.Data
{
    public class TMusicStreamingContext : DbContext
    {
        public TMusicStreamingContext(DbContextOptions<TMusicStreamingContext> options) : base(options) { }

        public DbSet<User>? Users { get; set; }
        public DbSet<Song>? Songs { get; set; }
        public DbSet<Album>? Albums { get; set; }
        public DbSet<Artist>? Artists { get; set; }
        public DbSet<Follow>? Follows { get; set; }
        public DbSet<Playlist>? Playlists { get; set; }
        public DbSet<PlaylistSong>? PlaylistSongs { get; set; }
        public DbSet<Favorite>? Favorites { get; set; }
        public DbSet<History>? Histories { get; set; }
        public DbSet<Comment>? Comments { get; set; }
        public DbSet<CommentReply>? CommentReplies { get; set; }
        public DbSet<CommentLike>? CommentLikes { get; set; }
        public DbSet<Genre>? Genres { get; set; }
        public DbSet<SongGenre>? SongGenres { get; set; }
        public DbSet<Download>? Downloads { get; set; }

        public DbSet<UserInteraction>? UserInteractions { get; set; }

        public DbSet<UserSimilarity>? UserSimilarities { get; set; }

        public DbSet<SongDTO> SongsDTO { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PlaylistSong>()
                .HasKey(ps => new { ps.PlaylistId, ps.SongId });

            modelBuilder.Entity<SongGenre>()
                .HasKey(sg => new { sg.SongId, sg.GenreId });

            modelBuilder.Entity<UserSimilarity>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Cấu hình relationship với User1
                entity.HasOne(e => e.User1)
                      .WithMany()
                      .HasForeignKey(e => e.UserId1)
                      .OnDelete(DeleteBehavior.Cascade);

                // Cấu hình relationship với User2  
                entity.HasOne(e => e.User2)
                      .WithMany()
                      .HasForeignKey(e => e.UserId2)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Follow
            modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.UserId, f.ArtistId });
            modelBuilder.Entity<Follow>()
                .HasOne(f => f.User)
                .WithMany(u => u.Follows)
                .HasForeignKey(f => f.UserId);
            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Artist)
                .WithMany(a => a.Followers)
                .HasForeignKey(f => f.ArtistId);

            // Comment relationships
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.Song)
                      .WithMany(s => s.Comments)
                      .HasForeignKey(c => c.SongId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // CommentReply relationships
            modelBuilder.Entity<CommentReply>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(cr => cr.Comment)
                      .WithMany(c => c.CommentReplies)
                      .HasForeignKey(cr => cr.CommentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cr => cr.User)
                      .WithMany()
                      .HasForeignKey(cr => cr.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(cr => cr.CommentId);
                entity.HasIndex(cr => cr.UserId);
            });

            // CommentLike relationships
            modelBuilder.Entity<CommentLike>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(cl => cl.Comment)
                      .WithMany(c => c.CommentLikes)
                      .HasForeignKey(cl => cl.CommentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cl => cl.User)
                      .WithMany()
                      .HasForeignKey(cl => cl.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(cl => new { cl.CommentId, cl.UserId }).IsUnique();
            });

            // Các indexes khác
            modelBuilder.Entity<UserSimilarity>()
                .HasIndex(us => us.UserId1);

            modelBuilder.Entity<UserSimilarity>()
                .HasIndex(us => us.UserId2);

            modelBuilder.Entity<UserSimilarity>()
                .HasIndex(us => new { us.UserId1, us.UserId2 })
                .IsUnique();

            modelBuilder.Entity<UserInteraction>()
                .HasIndex(ui => ui.UserId);

            modelBuilder.Entity<UserInteraction>()
                .HasIndex(ui => ui.SongId);

            modelBuilder.Entity<UserInteraction>()
                .HasIndex(ui => new { ui.UserId, ui.SongId });

            modelBuilder.Entity<History>()
                .HasIndex(h => h.UserId);

            modelBuilder.Entity<Favorite>()
                .HasIndex(f => f.UserId);

            modelBuilder.Entity<SongGenre>()
                .HasIndex(sg => sg.GenreId);

            modelBuilder.Entity<Song>()
                .HasIndex(s => s.ReleaseDate);

            modelBuilder.Entity<Album>()
                .HasIndex(a => a.ArtistId);

            // Bảng này không tạo -> Chỉ để truy vấn từ procedure
            modelBuilder.Entity<SongDTO>().HasNoKey();
        }
    }
}
