namespace Jellyfin.Plugin.SerializdSync.Api.Models;

/// <summary>
/// Represents a single episode log entry returned by the Serializd season page endpoint.
/// </summary>
public class EpisodeLogEntry
{
    /// <summary>
    /// Gets or sets the episode number.
    /// </summary>
    public int EpisodeNumber { get; set; }

    /// <summary>
    /// Gets or sets the date the episode was logged.
    /// </summary>
    public string? DateAdded { get; set; }
}
