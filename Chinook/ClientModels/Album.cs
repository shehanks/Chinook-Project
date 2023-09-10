﻿namespace Chinook.ClientModels;

public class Album
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public long ArtistId { get; set; }

    public Artist Artist { get; set; } = null!;
}
