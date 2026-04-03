using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.SerializdSync.Api.Models;
using Jellyfin.Plugin.SerializdSync.Models;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.SerializdSync.Api;

/// <summary>
/// Interface for the Serializd API client.
/// </summary>
public interface ISerializdApiClient
{
    /// <summary>
    /// Marks an episode as watched on Serializd.
    /// </summary>
    /// <param name="user">The Serializd user configuration.</param>
    /// <param name="episode">The episode to mark as watched.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    Task MarkEpisodeWatchedAsync(SerializdUser user, Episode episode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets show information from Serializd by TMDB ID.
    /// </summary>
    /// <param name="user">The Serializd user configuration.</param>
    /// <param name="tmdbShowId">The TMDB show ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The show information.</returns>
    Task<ShowInfoResponse?> GetShowInfoAsync(SerializdUser user, string tmdbShowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the season page data for a show, including the user's episode logs.
    /// </summary>
    /// <param name="user">The Serializd user configuration.</param>
    /// <param name="showId">The Serializd show ID.</param>
    /// <param name="seasonNumber">The season number.</param>
    /// <param name="seasonId">The Serializd season ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The season page data, or null if not found.</returns>
    Task<SeasonPageResponse?> GetSeasonPageAsync(SerializdUser user, int showId, int seasonNumber, int seasonId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates user credentials with Serializd.
    /// </summary>
    /// <param name="user">The Serializd user configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if credentials are valid, false otherwise.</returns>
    Task<bool> ValidateCredentialsAsync(SerializdUser user, CancellationToken cancellationToken = default);
}
