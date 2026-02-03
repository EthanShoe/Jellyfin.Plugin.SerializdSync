namespace Jellyfin.Plugin.SerializdSync.Api.Models;

/// <summary>
/// Request model for Serializd login.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public required string Password { get; set; }
}
