using AutoMapper;
using TMusicStreaming.DTOs.Album;
using TMusicStreaming.DTOs.Artist;
using TMusicStreaming.DTOs.Comment;
using TMusicStreaming.DTOs.Genre;
using TMusicStreaming.DTOs.Song;
using TMusicStreaming.DTOs.User;
using TMusicStreaming.Models;

namespace TMusicStreaming.Helpers
{
    public class TMusicStreamingMapper : Profile
    {
        public TMusicStreamingMapper()
        {
            CreateMap<Artist, ArtistDTO>();
            CreateMap<Genre, GenreDTO>();
            CreateMap<Album, AlbumDTO>();
            CreateMap<Song, SongDTO>();
            CreateMap<User, UserDTO>();
        }
    }
}
