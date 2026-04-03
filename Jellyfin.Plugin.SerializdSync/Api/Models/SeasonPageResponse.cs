#pragma warning disable CA1819

namespace Jellyfin.Plugin.SerializdSync.Api.Models;

/// <summary>
/// Response model for the Serializd season page endpoint.
/// </summary>
public class SeasonPageResponse
{
    /// <summary>
    /// Gets or sets the list of episodes the user has logged for this season.
    /// </summary>
    public EpisodeLogEntry[] EpisodeLogs { get; set; } = [];
}
