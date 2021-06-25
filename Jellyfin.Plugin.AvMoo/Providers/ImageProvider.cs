using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.AvMoo.Providers
{
    public class ImageProvider : IRemoteImageProvider
    {
        public string Name => "AvMoo Image Provider";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public ImageProvider(IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken) =>
            await _httpClientFactory.CreateClient(NamedClient.Default).GetAsync(url, cancellationToken);

        public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            //var list = new List<RemoteImageInfo>();

            //// 读取 AvMoo Id
            //var id = item.GetProviderId(Plugin.ProviderId);

            //// 如果 AvMoo Id 为空，则返回空列表
            //if (string.IsNullOrWhiteSpace(id))
            //{
            //    _logger.LogWarning($"GetImages failed because that the sid is empty: {item.Name}");
            //    return list;
            //}

            throw new NotImplementedException();
        }

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return new List<ImageType>
            {
                ImageType.Primary,
                ImageType.Backdrop,
                ImageType.Screenshot,
                ImageType.Thumb
            };
        }

        public bool Supports(BaseItem item)
        {
            return item is Movie || item is Series;
        }
    }
}
