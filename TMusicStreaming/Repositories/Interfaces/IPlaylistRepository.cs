using Microsoft.AspNetCore.Mvc;
using TMusicStreaming.DTOs.Playlist;
using TMusicStreaming.Models;

namespace TMusicStreaming.Repositories.Interfaces
{
    public interface IPlaylistRepository
    {
        Task<PlaylistDTO> GetPlaylistWithSongsAsync(int playlistId);
        Task<List<PlaylistPopularDTO>> GetPlaylistsPopularAsync(int page, int pageSize, string query);
        Task<int> GetPlaylistsPopularCountAsync(string query = "");
        Task<IEnumerable<PlaylistDTO>> GetPlaylistsByUserAsync(int userId);
        Task<bool> AddSongToPlaylistAsync(int playlistId, int songId);
        Task<bool> RemoveSongFromPlaylistAsync(int playlistId, int songId);
        Task AddAsync(Playlist playlist);
        Task UpdateAsync(int id, Playlist playlist);
        Task UpdatePrivacyAsync(int id, bool isDisplay);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
    }
}
