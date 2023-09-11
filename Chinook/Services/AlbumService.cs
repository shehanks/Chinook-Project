using AutoMapper;
using Chinook.ClientModels;
using Chinook.Providers.Contracts;
using Chinook.Services.Contracts;

namespace Chinook.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IAlbumProvider albumProvider;

        private readonly IMapper mapper;

        public AlbumService(IAlbumProvider albumProvider, IMapper mapper)
        {
            this.albumProvider = albumProvider;
            this.mapper = mapper;
        }

        public async Task<List<Album>> GetAlbumsByArtistId(int artistId)
        {
            var albums = await albumProvider.GetAlbumsByArtistId(artistId);
            return mapper.Map<List<Album>>(albums); ;
        }
    }
}
