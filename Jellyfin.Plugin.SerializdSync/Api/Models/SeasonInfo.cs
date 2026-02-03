namespace Jellyfin.Plugin.SerializdSync.Api.Models;

/// <summary>
/// Season information from Serializd.
/// </summary>
public class SeasonInfo
{
    /// <summary>
    /// Gets or sets the Serializd season ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the season number.
    /// </summary>
    public int SeasonNumber { get; set; }
}
