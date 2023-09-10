using AutoMapper;
using Chinook.ClientModels;
using Chinook.Providers.Contracts;
using Chinook.Services.Contracts;

namespace Chinook.Services
{
    public class ArtistService : IArtistService
    {
        private readonly IArtistProvider artistProvider;

        private readonly IMapper mapper;

        public ArtistService(IArtistProvider artistProvider, IMapper mapper)
        {
            this.artistProvider = artistProvider;
            this.mapper = mapper;
        }

        public async Task<Artist?> GetArtistById(long id)
        {
            var artist = await artistProvider.GetArtistById(id);
            return mapper.Map<Artist>(artist);
        }

        public async Task<List<Artist>> GetArtists()
        {
            var artists = await artistProvider.GetArtists();
            return mapper.Map<List<Artist>>(artists);
        }
    }
}
