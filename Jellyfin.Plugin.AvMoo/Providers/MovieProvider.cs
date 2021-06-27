using Jellyfin.Plugin.AvMoo.Dtos;
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
    public class MovieProvider : IHasOrder, IRemoteMetadataProvider<Series, SeriesInfo>, IRemoteMetadataProvider<Movie, MovieInfo>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public int Order => 3;

        public string Name => "AvMoo Movie Provider";

        public MovieProvider(IHttpClientFactory httpClientFactory, ILogger<MovieProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken) =>
            _httpClientFactory.CreateClient(NamedClient.Default).GetAsync(url, cancellationToken);

        public async Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken)
        {
            // 读取 AvMoo Id
            var id = info.GetProviderId(Plugin.ProviderId);

            // 如果 AvMoo Id 为空，则根据标题重新获取，且默认使用结果的第一条数据
            if (string.IsNullOrWhiteSpace(id))
            {
                var idList = await GetIdsAsync(info.Name, cancellationToken);

                if (idList.Count() > 0)
                {
                    id = idList.FirstOrDefault();
                }
                else
                {
                    return new MetadataResult<Movie>();
                }
            }

            // 获取 元数据
            var movie = await GetMetadata<Movie>(id, cancellationToken);

            if (movie != null && movie.HasMetadata)
            {
                // 如果能获取到元数据，则把 AvMoo Id 设置为 当前 id
                info.SetProviderId(Plugin.ProviderId, id);
            }

            return movie;
        }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
        {
            var results = new List<RemoteSearchResult>();
            var idList = new List<string>();

            // 获取 AvMoo Id
            var id = searchInfo.GetProviderId(Plugin.ProviderId);

            if (!string.IsNullOrEmpty(id))
            {
                // id 不为空，添加到 id 列表
                idList = new List<string>
                {
                    id
                };
            }
            else
            {
                // id 为空，则通过名称在线搜索并返回搜索结果的 id 列表
                idList = (List<string>)await GetIdsAsync(searchInfo.Name, cancellationToken).ConfigureAwait(false);
            }

            // 遍历 id 列表
            foreach (string idItem in idList)
            {
                // 获取 id 为 idItem 的影片详情
                var item = await GetDetailAsync(idItem, cancellationToken).ConfigureAwait(false);

                // 转换为 Jellyfin 查找结果(RemoteSearchResult)对象
                var searchResult = new RemoteSearchResult()
                {
                    Name = item.Title,
                    ImageUrl = item.Images.Large,
                    Overview = item.Intro
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

        public Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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

        /// <summary>
        /// 根据关键字搜索影片，并取得搜索结果的影片 id 列表。
        /// 例如 /movie/b1542bc3132897c7 中 /movie/id
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetIdsAsync(string key, CancellationToken cancellationToken)
        {
            var idList = new List<string>();

            // 查找页 url
            var url = $"https://{Plugin.Instance.Configuration.Domain}/{Plugin.Instance.Configuration.Language.ToString().ToLower()}/search/{key}";

            // 拉取 html
            var html = await GetHtmlAsync(url, cancellationToken);

            // 匹配影片
            var matches = Regex.Matches(html, Plugin.Instance.Configuration.SearchResultPattern); //@"/movie/([\d\w]+)"

            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    idList.Add(match.Groups[1].Value.Trim());
                }
            }

            // 返回 7 条记录
            return idList.Distinct().Take(7).ToList();
        }


        public async Task<MetadataResult<Movie>> GetMetadata<T>(string id, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Movie>();

            // 如果 id 为空，则直接返回空结果
            if (string.IsNullOrEmpty(id))
            {
                return result;
            }

            // 取得 影片详情
            var movie = await GetDetailAsync(id, cancellationToken);

            // 设置要返回的结果项
            result.Item = await TransMediaInfoAsync(movie, cancellationToken);

            // 添加 导演
            (await TransPersonInfoAsync(movie.Directors, PersonType.Director, cancellationToken))?.ForEach((item) =>
            {
                result.AddPerson(item);
            });

            // 添加 演员
            (await TransPersonInfoAsync(movie.Actresses, PersonType.Actor, cancellationToken))?.ForEach((item) =>
            {
                result.AddPerson(item);
            });

            // 添加 编剧
            //await TransPersonInfoAsync(movie.Writers, PersonType.Writer, cancellationToken).ForEach(result.AddPerson);

            result.QueriedById = true;
            result.HasMetadata = true;

            return result;
        }

        public async Task<MovieDetail> GetDetailAsync(string id, CancellationToken cancellationToken)
        {
            // id 不能为空
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogError("id is null.");
                throw new ArgumentException("sid is empty when getting subject");
            }

            // 详情页 url
            var url = $"https://{Plugin.Instance.Configuration.Domain}/{Plugin.Instance.Configuration.Language.ToString().ToLower()}/movie/{id}";

            // 拉取 html
            var html = await GetHtmlAsync(url, cancellationToken);

            var movie = new MovieDetail();

            // 设置 影片 主页
            movie.FromUrl = url;

            // 匹配 标题和番号
            var match = Regex.Match(html, Plugin.Instance.Configuration.TitlePattern); //@"<h3>(?<id>[A-Z\d\-]+)\s(?<title>.*?)</h3>"

            if (match.Success)
            {
                // 设置番号
                movie.AvId = match.Groups["id"].Value.Trim();

                // 设置标题
                movie.Title = match.Groups["title"].Value.Trim();
            }
            /*
            // 匹配 大封面
            match = Regex.Match(html, Plugin.Instance.Configuration.CoverPattern); //@"bigImage""\shref=""(?<large>.*?)"""

            if (match.Success)
            {
                movie.Images.Large = match.Groups["large"].Value.Trim();
            }
            */
            // 匹配 发行日期
            match = Regex.Match(html, Plugin.Instance.Configuration.ReleaseDatePattern); //@"发行时间:</span>\s(?<date>.*?)</p>"
            if (match.Success)
            {
                DateTime.TryParse(match.Groups["date"].Value.Trim(), out DateTime date);
                movie.ReleaseDate = date;
            }

            // 匹配 时长
            match = Regex.Match(html, Plugin.Instance.Configuration.DurationPattern); //@"长度:</span>\s(?<duration>\d+)"
            if (match.Success)
            {
                int.TryParse(match.Groups["duration"].Value.Trim(), out int duration);
                movie.Duration = duration;
            }

            // 匹配 导演
            match = Regex.Match(html, Plugin.Instance.Configuration.DirectorListPattern); //@"导演:</span>\s(?<directors>.*?)</p>"
            if (match.Success)
            {

                var items = Regex.Matches(match.Groups["directors"].Value.Trim(), Plugin.Instance.Configuration.DirectorPattern); //"href=\"(?<url>.*?)\">(?<name>.*?)<"
                foreach (Match item in items)
                {
                    if (item.Success)
                    {
                        movie.Directors.Add(new PersonModel
                        {
                            Name = item.Groups["name"].Value.Trim(),
                            Alt = item.Groups["url"].Value.Trim()
                        });
                    }
                }
                items = null;
            }

            // 匹配 工作室
            match = Regex.Match(html, Plugin.Instance.Configuration.StudioListPattern); //@"制作商:\s</p>\s*<p>(?<productors>.*?)\s*</p>"
            if (match.Success)
            {
                var items = Regex.Matches(match.Groups["productors"].Value.Trim(), Plugin.Instance.Configuration.StudioPattern); //"href=\"(?<url>.*?)\">(?<name>.*?)<"
                foreach (Match item in items)
                {
                    if (item.Success)
                    {
                        movie.Productors.Add(item.Groups["name"].Value.Trim());
                    }
                }
            }

            // 匹配 发行商
            match = Regex.Match(html, Plugin.Instance.Configuration.LabelListPattern); //@"发行商:\s</p>\s*<p>(?<publishers>.*?)\s*</p>"
            if (match.Success)
            {
                var items = Regex.Matches(match.Groups["publishers"].Value.Trim(), Plugin.Instance.Configuration.LabelPattern); //"href=\"(?<url>.*?)\">(?<name>.*?)<"
                foreach (Match item in items)
                {
                    if (item.Success)
                    {
                        movie.Publishers.Add(item.Groups["name"].Value.Trim());
                    }
                }
            }

            // 匹配 系列
            match = Regex.Match(html, Plugin.Instance.Configuration.CollectionListPattern); //@"系列:</p>\s*<p>(?<series>.*?)\s*</p>"
            if (match.Success)
            {
                var items = Regex.Matches(match.Groups["series"].Value, Plugin.Instance.Configuration.CollectionPattern); //"href=\"(?<url>.*?)\">(?<name>.*?)<"
                foreach (Match item in items)
                {
                    if (item.Success)
                    {
                        movie.Series.Add(item.Groups["name"].Value.Trim());
                    }
                }
            }

            // 匹配 类别
            match = Regex.Match(html, Plugin.Instance.Configuration.GenreListPattern); //@"类别:</p>\s*<p>(?<genres>.*?)\s*</p>"
            if (match.Success)
            {
                var items = Regex.Matches(match.Groups["genres"].Value, Plugin.Instance.Configuration.GenrePattern); //"href=\"(?<url>.*?)\">(?<name>.*?)<"
                foreach (Match item in items)
                {
                    if (item.Success)
                    {
                        movie.Genres.Add(item.Groups["name"].Value.Trim());
                    }
                }
            }

            // 匹配 演员
            match = Regex.Match(html, Plugin.Instance.Configuration.ActressListPattern); //@"avatar-waterfall"">\s*(?<actresses>[\w\W]*?)\s*</div>\s*<div"
            if (match.Success)
            {
                var items = Regex.Matches(match.Groups["actresses"].Value, Plugin.Instance.Configuration.ActressPattern); //@"href=""(?<url>.*?)""[\w\W]*?src=""(?<photo>.*?)""[\w\W]*?<span>(?<name>.*?)<"
                foreach (Match item in items)
                {
                    if (item.Success)
                    {
                        movie.Actresses.Add(new PersonModel
                        {
                            Name = item.Groups["name"].Value.Trim(),
                            Avatars = new Avatar { Large = item.Groups["photo"].Value.Trim() },
                            Alt = item.Groups["url"].Value.Trim()
                        });
                    }
                }
            }
            /*
            // 匹配 缩略图
            match = Regex.Match(html, Plugin.Instance.Configuration.ScreenshotListPattern); //@"sample-waterfall"">\s*(?<thumbnails>[\w\W]*?)\s*</div>\s*<div"
            if (match.Success)
            {
                var items = Regex.Matches(match.Groups["thumbnails"].Value, Plugin.Instance.Configuration.ScreenshotPattern); //"href=\"(?<url>.*?)\">(?<name>.*?)<"
                foreach (Match item in items)
                {
                    if (item.Success)
                    {
                        movie.Thumbnails.Add(item.Groups["url"].Value.Trim());
                    }
                }
            }
            */
            return movie;
        }

        /// <summary>
        /// 将 Dto 转为 Movie
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Movie> TransMediaInfoAsync(MovieDetail detail, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                // 设置 基础信息
                var movie = new Movie
                {
                    Name = detail.Title,
                    OriginalTitle = $"{detail.AvId} {detail.Title}",
                    SortName = detail.AvId,
                    ForcedSortName = detail.AvId,
                    Overview = detail.Intro,
                    HomePageUrl = detail.FromUrl
                };

                // 如果 发行日期 不为空
                if (detail.ReleaseDate != null)
                {
                    // 设置 发行日期
                    movie.PremiereDate = detail.ReleaseDate;
                    // 设置 年份
                    movie.ProductionYear = detail.ReleaseDate?.Year;
                }
                /*
                // 添加系列
                data.Series.ForEach((item) =>
                {
                    media.AddGenre($"S: {item}");
                });
                */
                // 添加类别
                detail.Genres.ForEach((item) =>
                {
                    movie.AddGenre(item);
                });

                // 添加 工作室
                detail.Productors.ForEach((item) =>
                {
                    movie.AddStudio(item);
                });

                // 添加 发行商
                detail.Publishers.ForEach((publisher) =>
                {
                    if (!movie.Studios.Contains(publisher))
                    {
                        movie.AddStudio(publisher);
                    }
                });

                return movie;

            }, cancellationToken);
        }

        /// <summary>
        /// 转换 Dto 到 人员列表
        /// </summary>
        /// <param name="people"></param>
        /// <param name="personType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<PersonInfo>> TransPersonInfoAsync(List<PersonModel> people, string personType, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var result = new List<PersonInfo>();

                foreach (var person in people)
                {
                    var personInfo = new PersonInfo
                    {
                        Name = person.Name,
                        Type = personType,
                        ImageUrl = person.Avatars?.Large
                    };

                    personInfo.SetProviderId(Plugin.ProviderId, person.Id);
                    result.Add(personInfo);
                }

                return result;

            }, cancellationToken);
        }
    }
}
