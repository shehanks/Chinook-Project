using Chinook.Models;
using Chinook.Providers.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Providers
{
    public class AlbumProvider : IAlbumProvider
    {
        private readonly IDbContextFactory<ChinookContext> dbContext;

        public AlbumProvider(IDbContextFactory<ChinookContext> dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<Album>> GetAlbumsByArtistId(int artistId)
        {
            var context = await dbContext.CreateDbContextAsync();
            return context.Albums.Where(a => a.ArtistId == artistId).ToList();
        }
    }
}
