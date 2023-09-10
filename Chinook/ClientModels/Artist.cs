namespace Chinook.ClientModels;

public class Artist
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public IList<Album>? Albums { get; set; }
}
