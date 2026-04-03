#pragma warning disable CA1819

using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.SerializdSync.Api.Models;

/// <summary>
/// Request model for marking episodes as watched.
/// </summary>
public class EpisodeLogRequest
{
    /// <summary>
    /// Gets or sets the Serializd show ID.
    /// </summary>
    public required int ShowId { get; set; }

    /// <summary>
    /// Gets or sets the Serializd season ID.
    /// </summary>
    public required int SeasonId { get; set; }

    /// <summary>
    /// Gets or sets the episode numbers to mark as watched.
    /// </summary>
    public required int[] EpisodeNumbers { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a rewatch.
    /// </summary>
    public bool IsRewatch { get; set; }
}
