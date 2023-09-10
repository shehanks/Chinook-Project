using Chinook.ClientModels;
using Chinook.Services.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Chinook.Pages
{
    public partial class PlaylistPage
    {
        [Parameter]
        public long PlaylistId { get; set; }

        [Inject]
        private IPlaylistService playlistService { get; set; }

        [Inject]
        private IRefreshService refreshService { get; set; }

        [Inject]
        private ILogger<Index> logger { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationState { get; set; }

        private Playlist playlistModel;
        private string currentUserId;
        private string infoMessage;

        protected override async Task OnInitializedAsync()
        {
            currentUserId = await GetUserId();
        }

        protected override async Task OnParametersSetAsync()
        {
            infoMessage = string.Empty;
            await InitializeContext();
        }

        private async Task<string> GetUserId()
        {
            var user = (await authenticationState).User;
            var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
            return userId;
        }

        private async void FavoriteTrack(long trackId)
        {
            var playlistTrack = playlistModel.Tracks?.FirstOrDefault(t => t.TrackId == trackId);

            if (playlistTrack != null && currentUserId != null)
            {
                try
                {
                    var userPlaylist = playlistService.AddTrackToUserFavorites(currentUserId, trackId);
                    if (userPlaylist.IsCompleted && userPlaylist.Result != null)
                    {
                        infoMessage = $"Track {playlistTrack?.ArtistName} - {playlistTrack?.AlbumTitle} - '{playlistTrack?.TrackName}' added to playlist Favorites.";
                        await InitializeContext();
                        refreshService.CallRequestRefresh();
                    }
                    else
                        infoMessage = "No changes have been made.";
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex.ToString());
                    throw;
                }
            }
        }

        private async void UnfavoriteTrack(long trackId)
        {
            var playlistTrack = playlistModel.Tracks?.FirstOrDefault(t => t.TrackId == trackId);

            if (playlistTrack != null && currentUserId != null)
            {
                try
                {
                    var deleted = playlistService.RemoveTrackFromFavoritePlaylist(currentUserId, trackId);
                    if (deleted.IsCompleted && deleted.Result)
                    {
                        infoMessage = $"Track {playlistTrack?.ArtistName} - {playlistTrack?.AlbumTitle} - '{playlistTrack?.TrackName}' removed from playlist Favorites.";
                        await InitializeContext();
                        refreshService.CallRequestRefresh();
                    }
                    else
                        infoMessage = "No changes have been made.";
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex.ToString());
                    throw;
                }
            }
        }

        private async void RemoveTrack(long playlistId, long trackId, bool isFavoritePlaylist)
        {
            if (playlistModel != null && playlistModel.Tracks != null)
            {
                var track = playlistModel.Tracks.FirstOrDefault(t => t.TrackId == trackId);
                if (track != null)
                {
                    try
                    {
                        if (isFavoritePlaylist)
                            await playlistService.RemoveTrackFromFavoritePlaylist(currentUserId, trackId);
                        else
                            await playlistService.RemoveTrackFromPlaylist(currentUserId, trackId, playlistId);
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, ex.ToString());
                        throw;
                    }

                    infoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} removed from playlist.";
                    await InitializeContext();
                    refreshService.CallRequestRefresh();
                }
            }
        }

        private void CloseInfoMessage()
        {
            infoMessage = "";
        }

        private async Task InitializeContext()
        {
            await InvokeAsync(StateHasChanged);
            try
            {
                playlistModel = playlistService.GetPlaylistModel(currentUserId, PlaylistId);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex.ToString());
                throw;
            }

            if (playlistModel != null && playlistModel.Tracks != null && !playlistModel.Tracks.Any())
                infoMessage = "There are no tracks in the playlist at the moment.";
        }
    }
}