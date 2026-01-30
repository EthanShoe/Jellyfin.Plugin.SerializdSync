using System;

namespace Jellyfin.Plugin.SerializdSync.Models;

/// <summary>
/// Represents a Serializd user configuration linked to a Jellyfin user.
/// </summary>
public class SerializdUser
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SerializdUser"/> class.
    /// </summary>
    public SerializdUser()
    {
        LinkedMbUserId = Guid.Empty;
        SerializdUsername = string.Empty;
        SerializdPassword = string.Empty;
    }

    /// <summary>
    /// Gets or sets the linked Jellyfin user ID.
    /// </summary>
    public Guid LinkedMbUserId { get; set; }

    /// <summary>
    /// Gets or sets the Serializd username.
    /// </summary>
    public string SerializdUsername { get; set; }

    /// <summary>
    /// Gets or sets the Serializd password.
    /// </summary>
    public string SerializdPassword { get; set; }
}
