namespace Jellyfin.Plugin.SerializdSync.Api.Models;

/// <summary>
/// Response model for Serializd login.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets the authentication token.
    /// </summary>
    public string? Token { get; set; }
}
