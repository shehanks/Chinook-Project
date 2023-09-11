using Chinook.Common;

namespace Chinook.Helpers
{
    public static class PlaylistHelper
    {
        public static string GetFavoritePlaylistName(string userId)
        {
            return $"{Constants.FavoritePlaylistName}.{userId}";
        }

        public static bool IsFavoritePlaylistName(string? playlistName, string userId)
        {
            if (playlistName == null) return false;
            return playlistName.EndsWith($".{userId}");
        }

        public static string? GetVisibleFavoritePlaylistName(string? playlistName, string userId)
        {
            if (playlistName == null) return null;
            return playlistName.EndsWith($".{userId}") ? Constants.FavoritePlaylistName : playlistName;
        }
    }
}
