using Chinook.ClientModels;

namespace Chinook.Services.Contracts
{
    public interface IAlbumService
    {
        Task<List<Album>> GetAlbumsByArtistId(int artistId);
    }
}
