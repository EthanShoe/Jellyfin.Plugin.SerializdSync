using System;
using System.Linq;
using Jellyfin.Plugin.SerializdSync.Models;

namespace Jellyfin.Plugin.SerializdSync.Helpers;

/// <summary>
/// Helper class for retrieving Serializd user configurations.
/// </summary>
public static class SerializdUserHelper
{
    /// <summary>
    /// Gets the Serializd user configuration for a given Jellyfin user.
    /// </summary>
    /// <param name="userGuid">The Jellyfin user GUID.</param>
    /// <param name="requireCredentials">If true, only returns users with configured credentials.</param>
    /// <returns>The SerializdUser if found, otherwise null.</returns>
    public static SerializdUser? GetSerializdUser(Guid userGuid, bool requireCredentials = false)
    {
        if (Plugin.Instance == null)
        {
            return null;
        }

        var serializdUsers = Plugin.Instance.Configuration.GetAllSerializdUsers();

        return serializdUsers.FirstOrDefault(user =>
        {
            if (user.LinkedMbUserId == Guid.Empty)
            {
                return false;
            }

            if (requireCredentials &&
                (string.IsNullOrWhiteSpace(user.SerializdUsername) ||
                 string.IsNullOrWhiteSpace(user.SerializdPassword)))
            {
                return false;
            }

            return user.LinkedMbUserId.Equals(userGuid);
        });
    }

    /// <summary>
    /// Checks if a Jellyfin user has Serializd credentials configured.
    /// </summary>
    /// <param name="userGuid">The Jellyfin user GUID.</param>
    /// <returns>True if credentials are configured, false otherwise.</returns>
    public static bool HasCredentials(Guid userGuid)
    {
        return GetSerializdUser(userGuid, requireCredentials: true) != null;
    }
}
