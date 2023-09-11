using AutoMapper;
using Chinook.Common;
using Chinook.Helpers;
using Chinook.Models;
using Chinook.Providers.Contracts;
using Chinook.Services.Contracts;
using Cm = Chinook.ClientModels;

namespace Chinook.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistProvider playlistProvider;

        private readonly IMapper mapper;

        public PlaylistService(IPlaylistProvider playlistProvider, IMapper mapper)
        {
            this.playlistProvider = playlistProvider;
            this.mapper = mapper;
        }

        public Cm.Playlist? GetPlaylistById(long id)
        {
            var playlist = playlistProvider.GetPlaylistById(id);
            return mapper.Map<Cm.Playlist>(playlist);
        }

        public Cm.Playlist? GetPlaylistModel(string userId, long id)
        {
            var playlistQueryData = playlistProvider.GetPlaylistQueryData(id);

            if (playlistQueryData == null)
                return null;

            Action<IMappingOperationOptions<Playlist, Cm.Playlist>> mappingOperationOptions = x => x.Items.Add(Constants.userIdName, userId);
            var playlist = playlistQueryData.Select(p =>
                mapper.Map(p, mappingOperationOptions)).FirstOrDefault();

            return playlist;
        }

        public Cm.Playlist? GetFavoritePlaylistByUser(string userId)
        {
            var playlist = playlistProvider.GetFavoritePlaylistByUser(userId);
            return mapper.Map<Cm.Playlist>(playlist);
        }

        public async Task<List<Cm.UserPlaylist>> GetUserPlayLists(string userId, bool skipFavorite)
        {
            var userPlaylists = await playlistProvider.GetUserPlayLists(userId);

            if (skipFavorite)
                userPlaylists.RemoveAll(x =>
                x.Playlist != null &&
                x.Playlist.Name != null &&
                x.Playlist.Name.Equals(PlaylistHelper.GetFavoritePlaylistName(userId)));

            var clientPlaylists = mapper.Map<List<Cm.UserPlaylist>>(userPlaylists);
            MoveFavoriteListToTop(userId, clientPlaylists);
            return clientPlaylists;
        }

        public async Task<Cm.UserPlaylist?> AddTrackToUserFavorites(string userId, long trackId)
        {
            var favoritePlaylist = playlistProvider.GetFavoritePlaylistByUser(userId);
            var favoritePlaylistName = PlaylistHelper.GetFavoritePlaylistName(userId);
            UserPlaylist? userPlaylist;

            if (favoritePlaylist == null)
                userPlaylist = await playlistProvider.AddTrackToNewPlayList(userId, trackId, favoritePlaylistName);
            else
                userPlaylist = await playlistProvider.AddTrackToExistingPlayList(userId, trackId, favoritePlaylist.PlaylistId);

            return mapper.Map<Cm.UserPlaylist>(userPlaylist);
        }

        public async Task<bool> RemoveTrackFromFavoritePlaylist(string userId, long trackId)
        {
            return await playlistProvider.RemoveTrackFromFavoritePlaylist(userId, trackId);
        }

        public async Task<bool> RemoveTrackFromPlaylist(string userId, long trackId, long playlistId)
        {
            return await playlistProvider.RemoveTrackFromPlaylist(userId, trackId, playlistId);
        }

        public async Task<Cm.UserPlaylist?> AddTrackToNewPlaylist(string userId, long trackId, string playlistName)
        {
            var userPlaylist = await playlistProvider.AddTrackToNewPlayList(userId, trackId, playlistName);
            return mapper.Map<Cm.UserPlaylist>(userPlaylist);
        }

        public async Task<Cm.UserPlaylist?> AddTrackToExistingPlaylist(string userId, long trackId, long playlistId)
        {
            var userPlaylist = await playlistProvider.AddTrackToExistingPlayList(userId, trackId, playlistId);
            return mapper.Map<Cm.UserPlaylist>(userPlaylist);
        }

        private void MoveFavoriteListToTop(string userId, List<Cm.UserPlaylist> clientPlaylists)
        {
            if (clientPlaylists != null && clientPlaylists.Any())
            {
                var favoriteList = clientPlaylists.FirstOrDefault(x => x.Playlist.Name == PlaylistHelper.GetFavoritePlaylistName(userId));
                if (favoriteList != null)
                {
                    clientPlaylists.Remove(favoriteList);
                    clientPlaylists.Insert(0, favoriteList);
                }
            }
        }
    }
}
