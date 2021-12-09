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

            // 生成 详情页 url
            var url = $"https://{Plugin.Instance.Configuration.Domain}/{Plugin.Instance.Configuration.Language.ToString().ToLower()}/movie/{id}";

            // 下载 详情页 html 源码
            var html = await _httpClientFactory.CreateClient(NamedClient.Default).GetStringAsync(url, cancellationToken);

            // 匹配 封面图 poster 和 fanart
            // 表达式为大封面 url，替换文件名中的 pl 为 ps 后为小封面图
            var pattern = Plugin.Instance.Configuration.CoverPattern; //@"bigImage""\shref=""(?<large>.*?)"""
            var match = Regex.Match(html, pattern);
            // 匹配到数据
            if (match.Success)
            {
                // 取得图片 url
                var imgUrl = match.Groups["large"].Value.Trim();

                _logger.LogInformation($"大封面 url: {imgUrl}");

                // 如果图片 url 不为空
                if (!string.IsNullOrEmpty(imgUrl))
                {
                    // 添加到图片列表
                    // 小封面 poster
                    list.Add(new RemoteImageInfo
                    {
                        ProviderName = Name,
                        Url = imgUrl.Replace("pl", "ps"),
                        Type = ImageType.Primary
                    });

                    // 大封面 fanart/backdrop
                    list.Add(new RemoteImageInfo
                    {
                        ProviderName = Name,
                        Url = imgUrl,
                        Type = ImageType.Backdrop
                    });

                    // 列表为“缩略图”显示时，显示大封面
                    list.Add(new RemoteImageInfo
                    {
                        ProviderName = Name,
                        Url = imgUrl,
                        Type = ImageType.Thumb
                    });
                }
            }

            // 匹配 预览图 screenshot
            // 预览图 列表表达式
            pattern = Plugin.Instance.Configuration.ScreenshotListPattern; //sample-waterfall"">\s*(?<thumbnails>[\w\W]*?)\s*</div>\s*<div
            // 匹配
            match = Regex.Match(html, pattern);  
            
            _logger.LogInformation("匹配预览图");

            // 匹配到数据
            if (match.Success)
            {
                // 获取 预览图 表达式
                pattern = Plugin.Instance.Configuration.ScreenshotPattern; //href="(?<url>.*?)"\stitle="(?<name>.*?)"

                _logger.LogInformation($"预览图表达式: {pattern}");

                // 匹配 预览图 列表
                var matches = Regex.Matches(match.Groups["thumbnails"].Value, pattern);

                _logger.LogInformation($"匹配到 {matches.Count} 预览图");

                // 遍历列表
                foreach (Match m in matches)
                {
                    // 匹配成功
                    if (m.Success)
                    {
                        // 图片 url
                        var imgUrl = m.Groups["url"].Value.Trim();

                        _logger.LogInformation($"缩略图: {imgUrl}");

                        list.Add(new RemoteImageInfo
                        {
                            ProviderName = Name,
                            Url = imgUrl,
                            Type = ImageType.Screenshot,
                            ThumbnailUrl = imgUrl.Replace("jp", "") // 缩略文件名
                        });
                    }
                }
            }

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
