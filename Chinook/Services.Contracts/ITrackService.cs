using Chinook.ClientModels;

namespace Chinook.Services.Contracts
{
    public interface ITrackService
    {
        Task<List<PlaylistTrack>?> GetPlaylistTracksByArtistId(long artistId, string userId);
    }
}
