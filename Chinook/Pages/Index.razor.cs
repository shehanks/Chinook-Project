using Chinook.ClientModels;
using Chinook.Services.Contracts;
using Microsoft.AspNetCore.Components;

namespace Chinook.Pages
{
    public partial class Index
    {
        private List<Artist>? Artists;

        [Inject]
        private IArtistService artistService { get; set; }

        [Inject]
        private ILogger<Index> logger { get; set; }

        private string? searchTerm { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await InitializeContext();
        }

        private async Task<List<Artist>?> GetArtists(string? artistNameSearchTerm = null)
        {
            try
            {
                var artists = await artistService.GetArtists();

                if (artistNameSearchTerm != null && artistNameSearchTerm.Length > 0 && artists != null)
                    Artists = artists.Where(x => x.Name != null && x.Name.Contains(artistNameSearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                else
                    Artists = artists;

                return Artists;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex.ToString());
                throw;
            }
        }

        private async void OnArtistSearch(ChangeEventArgs changeEvent)
        {
            if (changeEvent != null)
            {
                searchTerm = changeEvent.Value as string;
                await GetArtists(searchTerm);
            }
        }

        private async Task InitializeContext()
        {
            await InvokeAsync(StateHasChanged);
            Artists = await GetArtists();
        }
    }
}