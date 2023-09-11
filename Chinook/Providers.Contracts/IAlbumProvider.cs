using Chinook.Models;

namespace Chinook.Providers.Contracts
{
    public interface IAlbumProvider
    {
        Task<List<Album>> GetAlbumsByArtistId(int artistId);
    }
}
