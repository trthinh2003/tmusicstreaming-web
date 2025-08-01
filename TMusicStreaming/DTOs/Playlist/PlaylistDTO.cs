using TMusicStreaming.DTOs.Album;

namespace TMusicStreaming.DTOs.Playlist
{
    public class PlaylistDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public int UserId { get; set; }
        public int SongCount { get; set; } = 0;
        public bool isDisplay { get; set; } = false;

        public List<PlaylistSongDTO> Songs { get; set; } = new List<PlaylistSongDTO>();
    }

    public class PlaylistPopularDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? UserName { get; set; }
        public int SongCount { get; set; } = 0;
        public bool isDisplay { get; set; } = false;

        public List<PlaylistSongDTO> Songs { get; set; } = new List<PlaylistSongDTO>();
    }

    public class PlaylistCreateDTO
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
    }

    public class PlaylistUpdateDTO
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
    }

    public class UploadPlaylistImageDTO
    {
        public IFormFile? File { get; set; }
    }

    public class PlaylistSongDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Artist { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string? Cover { get; set; } = string.Empty;
        public string? Audio { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string? Background { get; set; } = string.Empty;
        public string? Lyric { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public AlbumDTO? Album { get; set; } = new AlbumDTO();
    }

    public class AddSongToPlaylistDTO
    {
        public int SongId { get; set; }
    }
    public class UpdatePrivacyDTO
    {
        public bool IsDisplay { get; set; }
    }

    public class RemoveSongFromPlaylistDTO
    {
        public int SongId { get; set; }
    }
}
