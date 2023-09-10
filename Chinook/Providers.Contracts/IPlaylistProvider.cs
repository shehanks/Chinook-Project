using Chinook.Models;

namespace Chinook.Providers.Contracts
{
    public interface IPlaylistProvider
    {
        Playlist? GetPlaylistById(long id);

        IQueryable<Playlist> GetPlaylistQueryData(long id);

        Task<List<UserPlaylist>> GetUserPlayLists(string userId);

        Playlist? GetFavoritePlaylistByUser(string userId);

        Task<UserPlaylist?> AddTrackToNewPlayList(string userId, long trackId, string playlistName);

        Task<UserPlaylist?> AddTrackToExistingPlayList(string userId, long trackId, long playlistId);

        Task<bool> RemoveTrackFromFavoritePlaylist(string userId, long trackId);

        Task<bool> RemoveTrackFromPlaylist(string userId, long trackId, long playlistId);
    }
}
