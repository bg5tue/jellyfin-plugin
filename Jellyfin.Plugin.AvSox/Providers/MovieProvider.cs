using Jellyfin.Plugin.AvSox.Dtos;
using Jellyfin.Plugin.AvSox.Utils;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.AvSox.Providers
{
    public class MovieProvider : IHasOrder, IRemoteMetadataProvider<Movie, MovieInfo>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MovieProvider> _logger;
        private readonly InfoHelper _infoHelper;
        
        public int Order => 3;

        public string Name => "AvSox Movie Provider";

        public MovieProvider(
            IHttpClientFactory httpClientFactory,
            ILogger<MovieProvider> logger,
            InfoHelper infoHelper)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _infoHelper = infoHelper;
        }

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken) =>
            _httpClientFactory.CreateClient(NamedClient.Default).GetAsync(url, cancellationToken);

        public async Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken)
        {
            // 读取 AvSox Id
            var id = info.GetProviderId(Plugin.ProviderId);

            // 如果 AvSox Id 为空，则根据标题重新获取，且默认使用结果的第一条数据
            if (string.IsNullOrWhiteSpace(id))
            {
                //var results = await GetIdsAsync(info.Name, cancellationToken);
                var results = (await _infoHelper.SearchMovieAsync(info.Name, cancellationToken).ConfigureAwait(false)).Select(item => item.Id);
                if (results.Count() > 0)
                {
                    id = results.FirstOrDefault();
                }
                else
                {
                    return new MetadataResult<Movie>();
                }
            }

            // 获取 元数据
            var movie = await _infoHelper.GetMetadata<Movie>(id, cancellationToken);

            if (movie != null && movie.HasMetadata)
            {
                // 如果能获取到元数据，则把 AvSox Id 设置为 当前 id
                info.SetProviderId(Plugin.ProviderId, id);
            }

            return movie;
        }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
        {
            var results = new List<RemoteSearchResult>();
            var ids = new List<string>();
            var searchResults = new List<SearchResult>();

            // 获取 AvSox Id
            var AvSoxId = searchInfo.GetProviderId(Plugin.ProviderId);

            if (!string.IsNullOrEmpty(AvSoxId))
            {
                // id 不为空，添加到 id 列表
                ids = new List<string>
                {
                    AvSoxId
                };
            }
            else
            {
                // id 为空，则通过名称在线搜索并返回搜索结果的 id 列表
                //ids = (List<string>)await GetIdsAsync(searchInfo.Name, cancellationToken).ConfigureAwait(false);
                searchResults = (List<SearchResult>)await _infoHelper.SearchMovieAsync(searchInfo.Name, cancellationToken).ConfigureAwait(false);
                ids = searchResults.Select(item => item.Id).ToList();
            }

            // 遍历 id 列表
            foreach (string id in ids)
            {
                // 获取 id 为 idItem 的影片详情
                var item = await _infoHelper.GetMovieDetailAsync(id, cancellationToken).ConfigureAwait(false);

                // 转换为 Jellyfin 查找结果(RemoteSearchResult)对象
                var searchResult = new RemoteSearchResult()
                {
                    Name = item.Title,
                    ImageUrl = item.Poster,
                    Overview = item.Intro,
                    SearchProviderName = Name
                };

                // 如果发行日期不为空，则设置年份
                if (item.ReleaseDate != null)
                {
                    searchResult.PremiereDate = item.ReleaseDate;
                    searchResult.ProductionYear = item.ReleaseDate?.Year;
                }

                // 设置 id
                searchResult.SetProviderId(Plugin.ProviderId, id);

                // 添加到搜索结果列表
                results.Add(searchResult);
            }

            return results;
        }

    }
}
