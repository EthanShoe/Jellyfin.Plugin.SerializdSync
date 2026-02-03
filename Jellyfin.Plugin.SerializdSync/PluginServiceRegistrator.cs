using Jellyfin.Plugin.SerializdSync.Api;
using Jellyfin.Plugin.SerializdSync.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.SerializdSync;

/// <summary>
/// Registers plugin services with Jellyfin's dependency injection container.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<ISerializdApiClient, SerializdApiClient>();
        serviceCollection.AddHostedService<PlaybackMonitor>();
    }
}
