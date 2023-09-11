using Chinook.ClientModels;

namespace Chinook.Services.Contracts
{
    public interface IPlaylistService
    {
        Playlist? GetPlaylistById(long id);

        Playlist? GetPlaylistModel(string userId, long id);

        Task<List<UserPlaylist>> GetUserPlayLists(string userId, bool skipFavorite);

        Task<UserPlaylist?> AddTrackToUserFavorites(string userId, long trackId);

        Playlist? GetFavoritePlaylistByUser(string userId);

        Task<bool> RemoveTrackFromFavoritePlaylist(string userId, long trackId);

        Task<bool> RemoveTrackFromPlaylist(string userId, long trackId, long playlistId);

        Task<UserPlaylist?> AddTrackToNewPlaylist(string userId, long trackId, string playlistName);

        Task<UserPlaylist?> AddTrackToExistingPlaylist(string userId, long trackId, long playlistId);
    }
}
