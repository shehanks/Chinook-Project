using AutoMapper;
using Chinook.Common;
using Chinook.Models;
using Chinook.Providers.Contracts;
using Chinook.Services.Contracts;
using Cm = Chinook.ClientModels;

namespace Chinook.Services
{
    public class TrackService : ITrackService
    {
        private readonly ITrackProvider trackProvider;

        private readonly IMapper mapper;

        public TrackService(ITrackProvider trackProvider, IMapper mapper)
        {
            this.trackProvider = trackProvider;
            this.mapper = mapper;
        }

        public async Task<List<Cm.PlaylistTrack>?> GetPlaylistTracksByArtistId(long artistId, string userId)
        {
            var tracks = await trackProvider.GetPlaylistTracks(artistId, userId);

            if (tracks != null && tracks.Any())
            {
                Action<IMappingOperationOptions<Track, Cm.PlaylistTrack>> mappingOperationOptions = option => option.Items.Add(Constants.userIdName, userId);
                var playlistTracks = tracks.AsQueryable().Select(t =>
                    mapper.Map(t, mappingOperationOptions)).ToList();

                return playlistTracks;
            }

            return null;
        }
    }
}
