using Jellyfin.Plugin.AvSox.Utils;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.AvSox
{
    /// <summary>
    /// Register tmdb services.
    /// </summary>
    public class ServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<InfoHelper>();
        }
    }
}
