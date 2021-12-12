using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.AVSOX.Utils
{
    /// <summary>
    /// 网络操作助手类
    /// </summary>
    public class HttpHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HttpHelper> _logger;

        public HttpHelper(
            IHttpClientFactory httpClientFactory,
            Logger<HttpHelper> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// 下载 html 源码
        /// </summary>
        /// <param name="url">要下载 html 源码的 url</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> GetHtmlAsync(string url, CancellationToken cancellationToken)
        {
            try
            {
                return await _httpClientFactory.CreateClient(NamedClient.Default).GetStringAsync(url, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Could not access url: {0}, status code: {1}", url, ex.StatusCode);
            }

            return string.Empty;
        }

    }
}
