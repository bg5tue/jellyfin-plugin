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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ImageProvider> _logger;

        public string Name => "AvMoo Image Provider";

        public ImageProvider(IHttpClientFactory httpClientFactory, ILogger<ImageProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClientFactory.CreateClient(NamedClient.Default).GetAsync(new Uri(url), cancellationToken);
        }

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            var list = new List<RemoteImageInfo>();

            // 读取 AvMoo Id
            var id = item.GetProviderId(Plugin.ProviderId);

            // 如果 AvMoo Id 为空，则返回空列表
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning($"GetImages failed because that the sid is empty: {item.Name}");
                return list;
            }

            var url = $"https://{Plugin.Instance.Configuration.Domain}/{Plugin.Instance.Configuration.Language}/movie/{id}";
            var html = await _httpClientFactory.CreateClient(NamedClient.Default).GetStringAsync(url, cancellationToken);




            return list;
        }

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            yield return ImageType.Primary;
            yield return ImageType.Backdrop;
            yield return ImageType.Screenshot;
            yield return ImageType.Thumb;

            //return new List<ImageType>
            //{
            //    ImageType.Primary,
            //    ImageType.Backdrop,
            //    ImageType.Screenshot,
            //    ImageType.Thumb
            //};

            //throw new NotImplementedException();
        }

        public bool Supports(BaseItem item)
        {
            return item is Movie;
        }
    }
}
