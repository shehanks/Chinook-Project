using Chinook.ClientModels;
using Chinook.Common;
using Chinook.Services.Contracts;
using Chinook.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Chinook.Pages
{
    public partial class ArtistPage
    {
        [Parameter]
        public long ArtistId { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState>? authenticationState { get; set; }

        [Inject]
        private IArtistService artistService { get; set; }

        [Inject]
        private ITrackService trackService { get; set; }

        [Inject]
        private IPlaylistService playlistService { get; set; }

        [Inject]
        private IRefreshService refreshService { get; set; }

        [Inject]
        private ILogger<Index> logger { get; set; }

        private Modal? playlistDialog { get; set; }
        private AddToPlaylistInput addToPlaylistInput = new AddToPlaylistInput();
        private EditContext? playlistEditContext;
        private Artist? artist;
        private List<UserPlaylist>? userPlaylists;
        private List<PlaylistTrack>? tracks;
        private PlaylistTrack? selectedTrack;
        private string? infoMessage;
        private string? currentUserId;
        private string? playlistDuplicationMessage;
        private bool isDuplicatePlaylistName;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(StateHasChanged);
            currentUserId = await GetUserId();
            try
            {
                artist = await artistService.GetArtistById(ArtistId);   
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex.ToString());
                throw;
            }

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
            var playlistTrack = tracks?.FirstOrDefault(t => t.TrackId == trackId);

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
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex.ToString());
                    throw;
                }
            }
            else
                infoMessage = "No changes have been made.";
        }

        private async void UnfavoriteTrack(long trackId)
        {
            var playlistTrack = tracks?.FirstOrDefault(t => t.TrackId == trackId);

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

        private void OpenPlaylistDialog(long trackId)
        {
            ResetPlaylistDuplicationMessage();
            addToPlaylistInput = new AddToPlaylistInput() { TrackId = trackId };
            CloseInfoMessage();
            selectedTrack = tracks?.FirstOrDefault(t => t.TrackId == trackId);
            playlistDialog?.Open();
        }

        private async void AddTrackToPlaylist(EditContext context)
        {
            if (context.Validate() && !isDuplicatePlaylistName)
            {
                playlistEditContext = new EditContext(addToPlaylistInput);
                var inputModel = playlistEditContext.Model as AddToPlaylistInput;

                if (inputModel == null || inputModel.TrackId <= 0 || currentUserId == null)
                    return;

                var trackId = inputModel.TrackId;
                try
                {
                    var existingPlaylist = playlistService.GetPlaylistById(inputModel.ExistingPlaylistId);
                    var isAddToExisting = existingPlaylist != null;
                    var isAlreadyAdded = existingPlaylist != null && existingPlaylist.Tracks.Any(a => a.TrackId == trackId);
                    var isAddToNew = inputModel.NewPlaylistName != null && !inputModel.NewPlaylistName.Trim().IsNullOrEmpty();

                    if (isAddToExisting && !isAlreadyAdded)
                        await playlistService.AddTrackToExistingPlaylist(currentUserId, trackId, inputModel.ExistingPlaylistId);

                    if (isAddToNew)
                        await playlistService.AddTrackToNewPlaylist(currentUserId, trackId, inputModel.NewPlaylistName);

                    CloseInfoMessage();

                    if (isAddToExisting || isAddToNew)
                    {
                        infoMessage = $"Track {artist?.Name} - {selectedTrack?.AlbumTitle} - '{selectedTrack?.TrackName}' added to playlist(s) " +
                            (isAddToNew ? $"'{inputModel.NewPlaylistName}'" : "") +
                            (isAddToExisting && !isAlreadyAdded ? $"& '{existingPlaylist?.Name}'" : "") +
                            (isAlreadyAdded ? $" - Track is already existed in the playlist '{existingPlaylist?.Name}'" : "");
                        await InitializeContext();
                        refreshService.CallRequestRefresh();
                    }
                    else
                        infoMessage = "No changes have been made.";
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex.ToString());
                    playlistDialog?.Close();
                    throw;
                }

                playlistDialog?.Close();
            }
        }

        private void CheckDuplicatePlaylistNames(ChangeEventArgs changeEvent)
        {
            isDuplicatePlaylistName = false;
            try
            {
                if (changeEvent != null && userPlaylists != null)
                {
                    var val = changeEvent.Value as string;

                    if (val != null && !val.Trim().IsNullOrEmpty())
                    {
                        val = val.Trim();
                        isDuplicatePlaylistName = userPlaylists.Any(x => x.Playlist != null &&
                        x.Playlist.Name != null &&
                        (x.Playlist.Name.Equals(val, StringComparison.OrdinalIgnoreCase) ||
                            Constants.FavoritePlaylistName.Equals(val, StringComparison.OrdinalIgnoreCase)));

                        playlistDuplicationMessage = isDuplicatePlaylistName ? "The value you entered is already existed in the playlists." : string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex.ToString());
                throw;
            }
        }

        private void CloseInfoMessage()
        {
            infoMessage = string.Empty;
        }

        private void ResetPlaylistDuplicationMessage()
        {
            playlistDuplicationMessage = string.Empty;
        }

        private async Task InitializeContext()
        {
            try
            {
                tracks = await trackService.GetPlaylistTracksByArtistId(ArtistId, currentUserId);
                userPlaylists = await playlistService.GetUserPlayLists(currentUserId, true);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex.ToString());
                throw;
            }
        }
    }
}