using Chinook.Models;

namespace Chinook.Providers.Contracts
{
    public interface IArtistProvider
    {
        Task<Artist?> GetArtistById(long id);
        Task<List<Artist>> GetArtists();
    }
}
