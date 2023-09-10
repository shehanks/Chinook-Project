using Chinook.Models;

namespace Chinook.Providers.Contracts
{
    public interface ITrackProvider
    {
        Task<IEnumerable<Track>?> GetPlaylistTracks(long artistId, string userId);
    }
}
