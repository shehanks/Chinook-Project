using Chinook.ClientModels;
using Chinook.Services.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Chinook.Shared
{
    public partial class NavMenu
    {
        [CascadingParameter]
        private Task<AuthenticationState>? authenticationState { get; set; }

        [Inject]
        private IPlaylistService playlistService { get; set; }

        [Inject]
        private IRefreshService refreshService { get; set; }

        [Inject]
        private ILogger<Index> logger { get; set; }

        private bool collapseNavMenu = true;

        private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;
        private List<UserPlaylist>? userPlaylists;
        private string? currentUserId;

        protected override async Task OnInitializedAsync()
        {
            await InitializeContext();
        }

        private void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }

        private async Task<string> GetUserId()
        {
            var user = (await authenticationState).User;
            var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
            return userId;
        }

        public async void Refresh()
        {
            if (currentUserId != null)
            {
                try
                {
                    userPlaylists = await playlistService.GetUserPlayLists(currentUserId, false);
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex.ToString());
                    throw;
                }

                StateHasChanged();
            }
        }

        private async Task InitializeContext()
        {
            await InvokeAsync(StateHasChanged);
            currentUserId = await GetUserId();
            try
            {
                userPlaylists = await playlistService.GetUserPlayLists(currentUserId, false);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex.ToString());
                throw;
            }

            refreshService.RefreshRequested += Refresh;
        }
    }
}