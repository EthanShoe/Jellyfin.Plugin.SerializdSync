#pragma warning disable CA1819

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.SerializdSync.Api.Models;

/// <summary>
/// Request model for publishing an episode review.
/// </summary>
public class EpisodeReviewRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EpisodeReviewRequest"/> class.
    /// </summary>
    /// <param name="showId">Tmdb show ID.</param>
    /// <param name="seasonId">Serializd episode ID.</param>
    /// <param name="episodeNumber">Episode number.</param>
    [SetsRequiredMembers]
    public EpisodeReviewRequest(int showId, int seasonId, int episodeNumber)
    {
        ShowId = showId;
        SeasonId = seasonId;
        ReviewText = string.Empty;
        Rating = 0;
        ContainsSpoiler = false;
        Backdate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
        IsLog = true;
        IsRewatch = false;
        EpisodeNumber = episodeNumber.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Tags = [];
        AllowsComments = true;
        Like = false;
    }

    /// <summary>
    /// Gets or sets the Serializd show ID.
    /// </summary>
    public required int ShowId { get; set; }

    /// <summary>
    /// Gets or sets the Serializd season ID.
    /// </summary>
    public required int SeasonId { get; set; }

    /// <summary>
    /// Gets or sets the review text.
    /// </summary>
    public required string ReviewText { get; set; }

    /// <summary>
    /// Gets or sets the rating.
    /// </summary>
    public required int Rating { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the review contains spoilers.
    /// </summary>
    public required bool ContainsSpoiler { get; set; }

    /// <summary>
    /// Gets or sets the backdate for the review.
    /// </summary>
    public required string Backdate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a log entry.
    /// </summary>
    public required bool IsLog { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this is a rewatch.
    /// </summary>
    public required bool IsRewatch { get; set; }

    /// <summary>
    /// Gets or sets the episode number.
    /// </summary>
    public required string EpisodeNumber { get; set; }

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    public required string[] Tags { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether comments are allowed.
    /// </summary>
    public required bool AllowsComments { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is liked.
    /// </summary>
    public required bool Like { get; set; }
}
