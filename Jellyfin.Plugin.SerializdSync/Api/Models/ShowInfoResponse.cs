#pragma warning disable CA1819

namespace Jellyfin.Plugin.SerializdSync.Api.Models;

/// <summary>
/// Response model for show information from Serializd.
/// </summary>
public class ShowInfoResponse
{
    /// <summary>
    /// Gets or sets the Serializd show ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the show name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the seasons.
    /// </summary>
    public SeasonInfo[] Seasons { get; set; } = [];
}
