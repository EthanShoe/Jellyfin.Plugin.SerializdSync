using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.SerializdSync.Api.Models;
using Jellyfin.Plugin.SerializdSync.Models;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SerializdSync.Api;

/// <summary>
/// Client for interacting with the Serializd API.
/// </summary>
public class SerializdApiClient : ISerializdApiClient, IDisposable
{
    private const string BaseUrl = "https://www.serializd.com/api/";
    private const int MaxRetries = 3;
    private const int BaseDelayMs = 1000;

    private readonly ILogger<SerializdApiClient> _logger;
    private readonly ConcurrentDictionary<Guid, UserAuthState> _authCache = new();
    private readonly SemaphoreSlim _rateLimiter = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializdApiClient"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public SerializdApiClient(ILogger<SerializdApiClient> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task MarkEpisodeWatchedAsync(SerializdUser user, Episode episode, CancellationToken cancellationToken = default)
    {
        JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(episode);

        // Get TMDB ID from the series
        var tmdbId = episode.Series?.GetProviderId("Tmdb");
        if (string.IsNullOrEmpty(tmdbId))
        {
            _logger.LogWarning(
                "Cannot mark episode as watched: Series {SeriesName} does not have a TMDB ID",
                episode.SeriesName);
            return;
        }

        var seasonNumber = episode.ParentIndexNumber;
        var episodeNumber = episode.IndexNumber;

        if (!seasonNumber.HasValue || !episodeNumber.HasValue)
        {
            _logger.LogWarning(
                "Cannot mark episode as watched: Missing season or episode number for {SeriesName}",
                episode.SeriesName);
            return;
        }

        // Get show info to find Serializd IDs
        var showInfo = await GetShowInfoAsync(user, tmdbId, cancellationToken).ConfigureAwait(false);
        if (showInfo == null)
        {
            _logger.LogWarning(
                "Cannot mark episode as watched: Show not found on Serializd for TMDB ID {TmdbId}",
                tmdbId);
            return;
        }

        // Find the season
        var season = showInfo.Seasons.FirstOrDefault(s => s.SeasonNumber == seasonNumber.Value);
        if (season == null)
        {
            _logger.LogWarning(
                "Cannot mark episode as watched: Season {SeasonNumber} not found for show {ShowName}",
                seasonNumber.Value,
                showInfo.Name);
            return;
        }

        // Add a log/"review"
        var episodeReviewRequest = new EpisodeReviewRequest(showInfo.Id, season.Id, episodeNumber.Value);

        await ExecuteWithRetryAsync(
            user,
            async (client, ct) =>
            {
                var response = await client.PostAsJsonAsync(
                    "show/reviews/add",
                    episodeReviewRequest,
                    jsonOptions,
                    ct).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                return true;
            },
            cancellationToken).ConfigureAwait(false);

        // Mark the episode as watched
        var episodeLogRequest = new EpisodeLogRequest
        {
            ShowId = showInfo.Id,
            SeasonId = season.Id,
            EpisodeNumbers = [episodeNumber.Value]
        };

        await ExecuteWithRetryAsync(
            user,
            async (client, ct) =>
            {
                var response = await client.PostAsJsonAsync(
                    "episode_log/add",
                    episodeLogRequest,
                    jsonOptions,
                    ct).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                return true;
            },
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Successfully marked as watched on Serializd: {SeriesName} S{Season:D2}E{Episode:D2}",
            episode.SeriesName,
            seasonNumber.Value,
            episodeNumber.Value);
    }

    /// <inheritdoc />
    public async Task<ShowInfoResponse?> GetShowInfoAsync(SerializdUser user, string tmdbShowId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(tmdbShowId);

        return await ExecuteWithRetryAsync(
            user,
            async (client, ct) =>
            {
                var response = await client.GetAsync($"show/{tmdbShowId}", ct).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                response.EnsureSuccessStatusCode();

                JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                return await response.Content.ReadFromJsonAsync<ShowInfoResponse>(jsonOptions, ct).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ValidateCredentialsAsync(SerializdUser user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        try
        {
            var authState = await GetOrCreateAuthStateAsync(user).ConfigureAwait(false);
            await EnsureAuthenticatedAsync(authState, user, forceReauth: true, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (SerializdApiException ex) when (ex.IsAuthenticationError)
        {
            _logger.LogWarning("Credential validation failed for user {Username}", user.SerializdUsername);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating credentials for user {Username}", user.SerializdUsername);
            return false;
        }
    }

    private async Task<T> ExecuteWithRetryAsync<T>(
        SerializdUser user,
        Func<HttpClient, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        await _rateLimiter.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var authState = await GetOrCreateAuthStateAsync(user).ConfigureAwait(false);

            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    await EnsureAuthenticatedAsync(authState, user, forceReauth: false, cancellationToken).ConfigureAwait(false);

                    return await operation(authState.HttpClient, cancellationToken).ConfigureAwait(false);
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogDebug("Authentication expired, re-authenticating (attempt {Attempt})", attempt + 1);

                    // Clear auth and retry
                    authState.IsAuthenticated = false;

                    if (attempt == MaxRetries - 1)
                    {
                        throw new SerializdApiException("Authentication failed after multiple attempts", HttpStatusCode.Unauthorized);
                    }
                }
                catch (HttpRequestException ex) when (IsTransientError(ex.StatusCode))
                {
                    if (attempt == MaxRetries - 1)
                    {
                        throw new SerializdApiException($"Request failed after {MaxRetries} attempts: {ex.Message}", ex);
                    }

                    var delay = TimeSpan.FromMilliseconds(BaseDelayMs * Math.Pow(2, attempt));
                    _logger.LogDebug("Transient error, retrying in {Delay}ms (attempt {Attempt})", delay.TotalMilliseconds, attempt + 1);
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
            }

            throw new SerializdApiException("Request failed after maximum retries");
        }
        finally
        {
            _rateLimiter.Release();
        }
    }

    private Task<UserAuthState> GetOrCreateAuthStateAsync(SerializdUser user)
    {
        if (_authCache.TryGetValue(user.LinkedMbUserId, out var existing))
        {
            return Task.FromResult(existing);
        }

        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            UseCookies = true,
            CheckCertificateRevocationList = true
        };

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(BaseUrl)
        };

        client.DefaultRequestHeaders.Add("X-Requested-With", "serializd_vercel");
        client.DefaultRequestHeaders.Add("Origin", "https://www.serializd.com");

        var authState = new UserAuthState
        {
            HttpClient = client,
            CookieContainer = cookieContainer,
            IsAuthenticated = false
        };

        _authCache[user.LinkedMbUserId] = authState;
        return Task.FromResult(authState);
    }

    private async Task EnsureAuthenticatedAsync(
        UserAuthState authState,
        SerializdUser user,
        bool forceReauth,
        CancellationToken cancellationToken)
    {
        JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        if (authState.IsAuthenticated && !forceReauth)
        {
            return;
        }

        _logger.LogDebug("Authenticating with Serializd as {Username}", user.SerializdUsername);

        var loginRequest = new LoginRequest
        {
            Email = user.SerializdUsername,
            Password = user.SerializdPassword
        };

        var response = await authState.HttpClient.PostAsJsonAsync("login", loginRequest, jsonOptions, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new SerializdApiException($"Login failed with status {response.StatusCode}", response.StatusCode);
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(jsonOptions, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(loginResponse?.Token))
        {
            throw new SerializdApiException("Login succeeded but no token received", HttpStatusCode.Unauthorized);
        }

        // Set the token as a cookie for authentication
        authState.CookieContainer.Add(
            new Uri("https://www.serializd.com"),
            new Cookie("tvproject_credentials", loginResponse.Token));

        authState.IsAuthenticated = true;
        _logger.LogDebug("Successfully authenticated with Serializd");
    }

    private static bool IsTransientError(HttpStatusCode? statusCode)
    {
        return statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;
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
            foreach (var authState in _authCache.Values)
            {
                authState.HttpClient.Dispose();
            }

            _authCache.Clear();
            _rateLimiter.Dispose();
        }

        _disposed = true;
    }

    private sealed class UserAuthState
    {
        public required HttpClient HttpClient { get; init; }

        public required CookieContainer CookieContainer { get; init; }

        public bool IsAuthenticated { get; set; }
    }
}
