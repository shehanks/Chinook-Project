using Chinook.Models;
using Chinook.Providers.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Providers
{
    public class TrackProvider : ITrackProvider
    {
        private readonly IDbContextFactory<ChinookContext> dbContext;

        public TrackProvider(IDbContextFactory<ChinookContext> dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<Track>?> GetPlaylistTracks(long artistId, string userId)
        {
            var context = await dbContext.CreateDbContextAsync();
            return context.Tracks.Include(a => a.Album).
                Include(p => p.Playlists).ThenInclude(up => up.UserPlaylists).
                Where(a => a.Album != null && a.Album.ArtistId == artistId);
        }
    }
}
