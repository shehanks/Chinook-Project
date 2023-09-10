namespace Chinook.ClientModels;
using System.ComponentModel.DataAnnotations;

public class AddToPlaylistInput
{
    [Range(1, long.MaxValue)]
    public long TrackId { get; set; }

    public long ExistingPlaylistId { get; set; }

    [RegularExpression(@"^(?!\s)([-_a-zA-Z0-9\s])*?(?<!\s)$", ErrorMessage = "Special characters and, leading and trailing spaces are not allowed.")]
    public string? NewPlaylistName { get; set; }
}
