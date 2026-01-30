#pragma warning disable CA1819

using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.SerializdSync.Models;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.SerializdSync.Configuration;

/// <summary>
/// Plugin configuration for SerializdSync.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        SerializdUsers = Array.Empty<SerializdUser>();
    }

    /// <summary>
    /// Gets or sets the Serializd users.
    /// </summary>
    public SerializdUser[] SerializdUsers { get; set; }

    /// <summary>
    /// Adds a user to the Serializd users.
    /// </summary>
    /// <param name="userGuid">The Jellyfin user GUID.</param>
    public void AddUser(Guid userGuid)
    {
        var users = SerializdUsers.ToList();
        var user = new SerializdUser
        {
            LinkedMbUserId = userGuid
        };
        users.Add(user);
        SerializdUsers = users.ToArray();
    }

    /// <summary>
    /// Removes a user from the Serializd users.
    /// </summary>
    /// <param name="userGuid">The Jellyfin user GUID.</param>
    public void RemoveUser(Guid userGuid)
    {
        var users = SerializdUsers.ToList();
        users.RemoveAll(user => user.LinkedMbUserId == userGuid);
        SerializdUsers = users.ToArray();
    }

    /// <summary>
    /// Gets a list of all Serializd users.
    /// </summary>
    /// <returns>A read-only list of all Serializd users.</returns>
    public IReadOnlyList<SerializdUser> GetAllSerializdUsers()
    {
        return SerializdUsers.ToList();
    }
}
