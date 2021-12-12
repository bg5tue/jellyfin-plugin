using Jellyfin.Plugin.AvSox.Utils;
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.AvSox.Providers
{
    public class MovieImageProvider : IRemoteImageProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MovieImageProvider> _logger;
        private readonly InfoHelper _infoHelper;

        public string Name => "AvSox Image Provider";

        public MovieImageProvider(
            IHttpClientFactory httpClientFactory,
            ILogger<MovieImageProvider> logger,
            InfoHelper infoHelper)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _infoHelper = infoHelper;
        }

        public Task<HttpResponseMessage> GetImageResponse(
            string url,
            CancellationToken cancellationToken)
                => _httpClientFactory.CreateClient(NamedClient.Default).GetAsync(new Uri(url), cancellationToken);

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(
            BaseItem item,
            CancellationToken cancellationToken)
        {
            var list = new List<RemoteImageInfo>();

            // 读取 AvSox Id
            var id = item.GetProviderId(Plugin.ProviderId);

            // 如果 AvSox Id 为空，则返回空列表
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning($"GetImages failed because that the sid is empty: {item.Name}");
                return list;
            }

            // 生成 详情页 url
            var url = $"https://{Plugin.Instance.Configuration.Domain}/{Plugin.Instance.Configuration.Language.ToString().ToLower()}/movie/{id}";

            // 下载 详情页 html 源码
            var html = await _httpClientFactory.CreateClient(NamedClient.Default).GetStringAsync(url, cancellationToken);

            // 获取 大封面 fanart URL
            var fanart = await _infoHelper.GetFanartAsync(html, cancellationToken);

            // 取得番号（获取小封面图 Poster 用）
            var avid = (await _infoHelper.GetTitleAndIdAsync(html, cancellationToken).ConfigureAwait(false)).id;

            _logger.LogInformation($"avid: {avid}");

            if (!string.IsNullOrEmpty(fanart))
            {
                // 如果图片 url 不为空
                if (!string.IsNullOrEmpty(fanart))
                {
                    // 添加到图片列表
                    // 小封面 poster
                    list.Add(new RemoteImageInfo
                    {
                        ProviderName = Name,
                        Url = await _infoHelper.GetPosterAsync(avid, cancellationToken),
                        Type = ImageType.Primary
                    });

                    // 大封面 fanart/backdrop
                    list.Add(new RemoteImageInfo
                    {
                        ProviderName = Name,
                        Url = fanart,
                        Type = ImageType.Backdrop
                    });

                    // 列表为“缩略图”显示时，显示大封面 landscape
                    list.Add(new RemoteImageInfo
                    {
                        ProviderName = Name,
                        Url = fanart,
                        Type = ImageType.Thumb
                    });
                }
            }

            var screenshots = await _infoHelper.GetScreenshotsAsync(html, Name, cancellationToken).ConfigureAwait(false);

            list.AddRange(screenshots);

            return list;
        }

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            yield return ImageType.Primary;
            yield return ImageType.Backdrop;
            yield return ImageType.Screenshot;
            yield return ImageType.Thumb;
        }

        public bool Supports(BaseItem item)
        {
            return item is Movie;
        }
    }
}
