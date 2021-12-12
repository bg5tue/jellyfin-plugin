using System;
using System.Collections.Generic;
using Jellyfin.Plugin.AvSox.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.AvSox
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public override string Name => "AvSox";

        public override Guid Id => Guid.Parse("e457fe3b-6b53-4242-878c-53ac6cc60136");

        public override string Description => "Get metadata for movies and other video content from AvSox.";

        public static Plugin Instance { get; private set; }

        public static string ProviderId => "AvSox Id";        

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) 
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = this.Name,
                    EmbeddedResourcePath = string.Format("{0}.Configuration.configPage.html", GetType().Namespace)
                }
            };
        }
    }
}
