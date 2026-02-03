using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.SerializdSync.Api;
using Jellyfin.Plugin.SerializdSync.Api.Models;
using Jellyfin.Plugin.SerializdSync.Helpers;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SerializdSync.Services;

/// <summary>
/// Monitors playback events to detect when users finish watching TV episodes.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PlaybackMonitor"/> class.
/// </remarks>
/// <param name="sessionManager">The session manager.</param>
/// <param name="serializdApi">The Serializd API client.</param>
/// <param name="logger">The logger.</param>
public class PlaybackMonitor(
    ISessionManager sessionManager,
    ISerializdApiClient serializdApi,
    ILogger<PlaybackMonitor> logger) : IHostedService, IDisposable
{
    private readonly ISessionManager _sessionManager = sessionManager;
    private readonly ISerializdApiClient _serializdApi = serializdApi;
    private readonly ILogger<PlaybackMonitor> _logger = logger;
    private bool _disposed;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _sessionManager.PlaybackStopped += OnPlaybackStopped;
        _logger.LogInformation("SerializdSync playback monitor started");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _sessionManager.PlaybackStopped -= OnPlaybackStopped;
        _logger.LogInformation("SerializdSync playback monitor stopped");
        return Task.CompletedTask;
    }

    private async void OnPlaybackStopped(object? sender, PlaybackStopEventArgs e)
    {
        try
        {
            await HandlePlaybackStoppedAsync(e).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling playback stopped event");
        }
    }

    private async Task HandlePlaybackStoppedAsync(PlaybackStopEventArgs e)
    {
        // Only process if played to completion
        if (!e.PlayedToCompletion)
        {
            return;
        }

        // Only process TV episodes
        if (e.Item is not Episode episode)
        {
            return;
        }

        // Ensure we have user information
        if (e.Users == null || e.Users.Count == 0)
        {
            _logger.LogDebug("No users associated with playback event");
            return;
        }

        foreach (var user in e.Users)
        {
            // Check if this user has Serializd credentials configured
            var serializdUser = SerializdUserHelper.GetSerializdUser(user.Id, requireCredentials: true);
            if (serializdUser == null)
            {
                _logger.LogDebug(
                    "User {Username} does not have Serializd credentials configured, skipping",
                    user.Username);
                continue;
            }

            var seriesName = episode.SeriesName ?? "Unknown Series";
            var seasonNumber = episode.ParentIndexNumber ?? 0;
            var episodeNumber = episode.IndexNumber ?? 0;
            var episodeName = episode.Name ?? "Unknown Episode";

            _logger.LogInformation(
                "User {Username} completed watching: {SeriesName} S{Season:D2}E{Episode:D2} - {EpisodeName}",
                user.Username,
                seriesName,
                seasonNumber,
                episodeNumber,
                episodeName);

            try
            {
                await _serializdApi.MarkEpisodeWatchedAsync(serializdUser, episode).ConfigureAwait(false);
            }
            catch (SerializdApiException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to mark episode as watched on Serializd for user {Username}: {Message}",
                    user.Username,
                    ex.Message);
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and optionally managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Dispose managed resources if any
        }

        _disposed = true;
    }
}
