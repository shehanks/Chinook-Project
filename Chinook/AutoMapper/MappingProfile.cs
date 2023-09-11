using AutoMapper;
using Chinook.Common;
using Chinook.Helpers;
using Chinook.Models;
using Cm = Chinook.ClientModels;

namespace Chinook.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            string userId = string.Empty;

            CreateMap<Track, Cm.PlaylistTrack>()
                .ForMember(d => d.AlbumTitle, opt => opt.MapFrom(t => t.Album.Title))
                .ForMember(d => d.ArtistName, opt => opt.MapFrom(t => t.Album.Artist.Name))
                .ForMember(d => d.TrackName, opt => opt.MapFrom(t => t.Name))
                .AfterMap((src, des, context) =>
                {
                    var contextValues = new Dictionary<string, object>();

                    if (context.TryGetItems(out contextValues))
                    {
                        object? userId = null;
                        var hasUserId = contextValues != null && contextValues.Any() && contextValues.TryGetValue(Constants.userIdName, out userId);
                        if (hasUserId && userId != null)
                        {
                            des.IsFavorite = src.Playlists
                            .Where(
                                p => p.UserPlaylists != null &&
                                p.UserPlaylists.Any(up =>
                                    up.UserId == (string)userId &&
                                    up.Playlist.Name == PlaylistHelper.GetFavoritePlaylistName((string)userId))
                                ).Any();
                        }
                    }
                });

            CreateMap<Playlist, Cm.Playlist>()
                .ForMember(d => d.Id, opt => opt.MapFrom(p => p.PlaylistId));

            CreateMap<Album, Cm.Album>()
                .ForMember(d => d.Id, opt => opt.MapFrom(a => a.AlbumId));

            CreateMap<Artist, Cm.Artist>()
                .ForMember(d => d.Id, opt => opt.MapFrom(a => a.ArtistId));

            CreateMap<UserPlaylist, Cm.UserPlaylist>();
        }
    }
}
