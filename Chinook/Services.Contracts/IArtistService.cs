using Chinook.ClientModels;

namespace Chinook.Services.Contracts
{
    public interface IArtistService
    {
        Task<List<Artist>> GetArtists();

        Task<Artist?> GetArtistById(long id);
    }
}
