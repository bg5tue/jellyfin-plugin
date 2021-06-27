using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.AvMoo
{
    public class ExternalId : IExternalId
    {
        public string ProviderName => Plugin.Instance.Name;

        public string Key => Plugin.ProviderId;

        public ExternalIdMediaType? Type => null;

        public string UrlFormatString => $"https://{Plugin.Instance.Configuration.Domain}/{Plugin.Instance.Configuration.Language.ToString().ToLower()}/movie/{{0}}";

        public bool Supports(IHasProviderIds item)
        {
            return item is Movie || item is Video;
        }
    }
}
