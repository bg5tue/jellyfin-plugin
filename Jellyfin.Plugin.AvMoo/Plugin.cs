using System;
using System.Collections.Generic;
using Jellyfin.Plugin.AvMoo.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.AvMoo
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public override string Name => "AvMoo";

        public override Guid Id => Guid.Parse("52eb4c4a-d4bc-11eb-b616-000c290aa604");

        public override string Description => "Get metadata for movies and other video content from AvMoo.";

        public static Plugin Instance { get; private set; }

        public static string ProviderId => "AvMoo Id";        

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
