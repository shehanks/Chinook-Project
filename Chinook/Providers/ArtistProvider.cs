using Chinook.Models;
using Chinook.Providers.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Providers
{
    public class ArtistProvider : IArtistProvider
    {
        private readonly IDbContextFactory<ChinookContext> dbContext;

        public ArtistProvider(IDbContextFactory<ChinookContext> dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Artist?> GetArtistById(long id)
        {
            var context = await dbContext.CreateDbContextAsync();
            return context.Artists.SingleOrDefault(a => a.ArtistId == id);
        }

        public async Task<List<Artist>> GetArtists()
        {
            var context = await dbContext.CreateDbContextAsync();
            return context.Artists.Include(a => a.Albums).ToList();
        }
    }
}
