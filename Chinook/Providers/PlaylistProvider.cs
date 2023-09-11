using Chinook.Helpers;
using Chinook.Models;
using Chinook.Providers.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Providers
{
    public class PlaylistProvider : IPlaylistProvider
    {
        private readonly IDbContextFactory<ChinookContext> dbContext;

        public PlaylistProvider(IDbContextFactory<ChinookContext> dbContext)
        {
            this.dbContext = dbContext;
        }

        public Playlist? GetPlaylistById(long id)
        {
            var context = dbContext.CreateDbContext();
            return context.Playlists.Include(a => a.Tracks).FirstOrDefault(x => x.PlaylistId == id);
        }

        public IQueryable<Playlist> GetPlaylistQueryData(long id)
        {
            var context = dbContext.CreateDbContext();
            return context.Playlists
            .Include(t => t.Tracks).ThenInclude(al => al.Playlists).ThenInclude(ar => ar.UserPlaylists)
            .Include(t => t.Tracks).ThenInclude(al => al.Album).ThenInclude(ar => ar.Artist)
            .Include(u => u.UserPlaylists)
            .Where(p => p.PlaylistId == id);
        }

        public Playlist? GetFavoritePlaylistByUser(string userId)
        {
            var context = dbContext.CreateDbContext();
            var favoritePlaylistName = PlaylistHelper.GetFavoritePlaylistName(userId);
            return context.Playlists.
                    Include(t => t.Tracks).
                    SingleOrDefault(x => x.Name != null && x.Name.Equals(favoritePlaylistName));
        }

        public async Task<List<UserPlaylist>> GetUserPlayLists(string userId)
        {
            var context = await dbContext.CreateDbContextAsync();
            var list = context.UserPlaylists.Where(x => x.UserId == userId)
                .Include(x => x.Playlist)
                .ThenInclude(x => x.Tracks)
                .OrderBy(x => x.PlaylistId)
                .ToList();

            return list;
        }

        public async Task<UserPlaylist?> AddTrackToExistingPlayList(string userId, long trackId, long playlistId)
        {
            var context = await dbContext.CreateDbContextAsync();

            var userPlaylist = context.Playlists.
                    Include(t => t.Tracks).
                    SingleOrDefault(x => x.Name != null && x.PlaylistId == playlistId);
            var track = context.Tracks.First(x => x.TrackId == trackId);

            if (track == null || context == null || userPlaylist == null)
                return null;

            context.Entry(userPlaylist).State = EntityState.Detached;
            var existingTracks = userPlaylist.Tracks;

            if (existingTracks == null || !existingTracks.Any())
                userPlaylist.Tracks = new List<Track>() { track };
            else
                userPlaylist.Tracks.Add(track);

            var playlist = context.Update(userPlaylist);
            context.SaveChanges();

            return context.UserPlaylists.First(x => x.UserId.Equals(userId) && x.PlaylistId == playlist.Entity.PlaylistId);
        }

        public async Task<UserPlaylist?> AddTrackToNewPlayList(string userId, long trackId, string playlistName)
        {
            var context = await dbContext.CreateDbContextAsync();
            var lastPlaylistId = context.Playlists.Max(m => m.PlaylistId);
            var track = context.Tracks.First(x => x.TrackId == trackId);

            if (track == null || context == null || lastPlaylistId <= 0)
                return null;

            var newUserPlayList = GetNewUserPlaylistModel(userId, lastPlaylistId, track, playlistName);
            var entity = context.UserPlaylists.Add(newUserPlayList);
            context.SaveChanges();

            return entity.Entity;
        }

        public async Task<bool> RemoveTrackFromFavoritePlaylist(string userId, long trackId)
        {
            var context = await dbContext.CreateDbContextAsync();
            var userFavoritePlaylist = context.Playlists.
                SingleOrDefault(x => x.Name != null && x.Name.Equals(PlaylistHelper.GetFavoritePlaylistName(userId)));

            return RemoveTrack(context, userId, trackId, userFavoritePlaylist);
        }

        public async Task<bool> RemoveTrackFromPlaylist(string userId, long trackId, long playlistId)
        {
            var context = await dbContext.CreateDbContextAsync();
            var userFavoritePlaylist = context.Playlists.
                SingleOrDefault(x => x.PlaylistId == playlistId);

            return RemoveTrack(context, userId, trackId, userFavoritePlaylist);
        }

        private UserPlaylist GetNewUserPlaylistModel(string userId, long lastPlaylist, Track track, string playlistName)
        {
            return new UserPlaylist()
            {
                UserId = userId,
                PlaylistId = lastPlaylist + 1,
                Playlist = new Models.Playlist()
                {
                    PlaylistId = lastPlaylist + 1,
                    Tracks = new List<Track>() { track },
                    Name = playlistName
                }
            };
        }

        private bool RemoveTrack(ChinookContext context, string userId, long trackId, Playlist? userFavoritePlaylist)
        {
            var track = context.Tracks.First(x => x.TrackId == trackId);

            if (track == null || context == null || userFavoritePlaylist == null)
                return false;

            context.Entry(userFavoritePlaylist).State = EntityState.Detached;

            var userPlaylist = context.UserPlaylists
                        .Include(p => p.Playlist)
                        .Include(t => t.Playlist.Tracks)
                        .FirstOrDefault(x => x.UserId.Equals(userId) && x.PlaylistId == userFavoritePlaylist.PlaylistId);

            if (userPlaylist != null)
            {
                userPlaylist.Playlist.Tracks.Remove(track);
                context.Update(userPlaylist);
                context.SaveChanges();
                return true;
            }

            return false;
        }
    }
}
