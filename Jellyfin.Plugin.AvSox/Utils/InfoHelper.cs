using Jellyfin.Plugin.AvSox.Dtos;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
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

namespace Jellyfin.Plugin.AvSox.Utils
{
    /// <summary>
    /// 信息处理助手类
    /// </summary>
    public class InfoHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<InfoHelper> _logger;

        public InfoHelper(
            IHttpClientFactory httpClientFactory,
            ILogger<InfoHelper> logger)
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

        /// <summary>
        /// 根据关键字搜索影片，并取得搜索结果列表。
        /// 例如 /movie/b1542bc3132897c7 中 /movie/后面的 b1542bc3132897c7
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<SearchResult>> SearchMovieAsync(string key, CancellationToken cancellationToken)
        {
            var results = new List<SearchResult>();

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
                    results.Add(new SearchResult
                    {
                        Id = match.Groups["id"].Value.Trim(),
                        Poster = match.Groups["poster"].Value.Trim(),
                        Title = match.Groups["title"].Value.Trim(),
                        ReleaseData = match.Groups["date"].Value.Trim(),
                        Avid = match.Groups["avid"].Value.Trim()
                    });
                }
            }

            // 返回 30 条记录
            return results.Distinct().Take(30).ToList();
        }

        /// <summary>
        /// 获取影片详情
        /// </summary>
        /// <param name="id">编号（非番号）</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<MovieDetail> GetMovieDetailAsync(string id, CancellationToken cancellationToken)
        {
            // id 不能为空
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogError("id is null.");
                throw new ArgumentException("sid is empty when getting subject");
            }
            _logger.LogInformation($"get movie detail: {id}");
            // 详情页 url
            var url = $"https://{Plugin.Instance.Configuration.Domain}/{Plugin.Instance.Configuration.Language.ToString().ToLower()}/movie/{id}";

            // 拉取 html
            var html = await GetHtmlAsync(url, cancellationToken);

            var ti = await GetTitleAndIdAsync(html, cancellationToken);
            _logger.LogInformation($"get movie detail: {ti.id}");
            return new MovieDetail
            {
                AvId = ti.id,
                Title = ti.title,
                Poster = await GetPosterAsync(ti.id, cancellationToken),
                Fanart = await GetFanartAsync(html, cancellationToken),
                ReleaseDate = GetReleaseDate(html),
                Duration = GetDuration(html),
                Directors = await GetDirectorsAsync(html, cancellationToken),
                Studios = await GetStudiosAsync(html, cancellationToken),
                Labels = await GetLabelsAsync(html, cancellationToken),
                Genres = await GetGenresAsync(html, cancellationToken),
                Actresses = await GetActressesAsync(html, cancellationToken),
                FromUrl = url
            };
        }

        /// <summary>
        /// 获取标题和番号
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public async Task<(string title, string id)> GetTitleAndIdAsync(
            string html,
            CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var match = Regex.Match(html, Plugin.Instance.Configuration.TitlePattern); //@"<h3>(?<id>[A-Za-z\d\-]+)\s(?<title>.*?)</h3>"

                _logger.LogInformation($"GetTitleAndIdAsync() => title pattern: {Plugin.Instance.Configuration.TitlePattern}");

                _logger.LogInformation($"GetTitleAndIdAsync() => match: {match.Success}");

                if (match.Success)
                {
                    var id1=match.Groups["id"].Value.Trim();
                    _logger.LogInformation($"GetTitleAndIdAsync() => id: {id1}");
                    var title1=match.Groups["title"].Value.Trim();
                    _logger.LogInformation($"GetTitleAndIdAsync() => title: {title1}");
                    return (title1, id1);
                }
                return (null, null);
            }, cancellationToken);
        }

        /// <summary>
        /// 获取小封面 URL，
        /// 由于 AvSox 大小封面的 URL 没有规律，
        /// 只能通过搜索页重新搜索 番号 来获取小封面
        /// </summary>
        /// <param name="id">番号</param>
        /// <returns></returns>
        public async Task<string> GetPosterAsync(
            string id,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation($"get poster, id: {id}");
            var items = await SearchMovieAsync(id, cancellationToken).ConfigureAwait(false);

            var posters = items.Select(x => x.Poster).ToList();
            _logger.LogInformation($"poster list: {string.Join(",", posters)}");

            return items.FirstOrDefault().Poster;
        }

        /// <summary>
        /// 获取大封面 URL
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public async Task<string> GetFanartAsync(
            string html,
            CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var match = Regex.Match(html, Plugin.Instance.Configuration.CoverPattern); //@"bigImage""\shref=""(?<large>.*?)"""

                if (match.Success)
                {
                    return match.Groups["large"].Value.Trim();
                }
                return string.Empty;
            }, cancellationToken);
        }

        /// <summary>
        /// 获取发行日期
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public DateTime? GetReleaseDate(string html)
        {
            var match = Regex.Match(html, Plugin.Instance.Configuration.ReleaseDatePattern); //@"发行时间:</span>\s(?<date>.*?)</p>"
            if (match.Success)
            {
                DateTime.TryParse(match.Groups["date"].Value.Trim(), out DateTime date);
                return date;
            }
            return null;
        }

        /// <summary>
        /// 获取时长
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public int GetDuration(string html)
        {
            var match = Regex.Match(html, Plugin.Instance.Configuration.DurationPattern); //@"长度:</span>\s(?<duration>\d+)"
            if (match.Success)
            {
                int.TryParse(match.Groups["duration"].Value.Trim(), out int duration);
                return duration;
            }

            return 0;
        }

        /// <summary>
        /// 获取导演列表
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public async Task<List<PersonModel>> GetDirectorsAsync(
            string html,
            CancellationToken cancellationToken)
        {
            var list = new List<PersonModel>();
            return await Task.Run(() =>
            {
                var match = Regex.Match(html, Plugin.Instance.Configuration.DirectorListPattern); //@"导演:</span>\s(?<directors>.*?)</p>"
                if (match.Success)
                {

                    var items = Regex.Matches(match.Groups["directors"].Value.Trim(), Plugin.Instance.Configuration.DirectorPattern); //"href=\"(?<url>.*?)\">(?<name>.*?)<"
                    foreach (Match item in items)
                    {
                        if (item.Success)
                        {
                            list.Add(new PersonModel
                            {
                                Name = item.Groups["name"].Value.Trim(),
                                Alt = item.Groups["url"].Value.Trim()
                            });
                        }
                    }
                    items = null;
                }

                return list;
            }, cancellationToken);
        }

        /// <summary>
        /// 获取工作室列表
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public async Task<List<string>> GetStudiosAsync(
            string html,
            CancellationToken cancellationToken)
        {
            var list = new List<string>();
            return await Task.Run(() =>
            {
                var match = Regex.Match(html, Plugin.Instance.Configuration.StudioListPattern); //@"制作商:\s</p>\s*<p>(?<studios>.*?)\s*</p>"
                if (match.Success)
                {
                    var items = Regex.Matches(match.Groups["studios"].Value.Trim(), Plugin.Instance.Configuration.StudioPattern); //"href=\"(?<url>.*?)\">(?<name>.*?)<"
                    foreach (Match item in items)
                    {
                        if (item.Success)
                        {
                            list.Add(item.Groups["name"].Value.Trim());
                        }
                    }
                }

                return list;
            }, cancellationToken);
        }

        /// <summary>
        /// 获取发行商列表
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public async Task<List<string>> GetLabelsAsync(
            string html,
            CancellationToken cancellationToken)
        {
            var list = new List<string>();
            return await Task.Run(() =>
            {
                var match = Regex.Match(html, Plugin.Instance.Configuration.LabelListPattern); //@"发行商:\s</p>\s*<p>(?<labels>.*?)\s*</p>"
                if (match.Success)
                {
                    var items = Regex.Matches(match.Groups["labels"].Value.Trim(), Plugin.Instance.Configuration.LabelPattern); //"href=\"(?<url>.*?)\">(?<name>.*?)<"
                    foreach (Match item in items)
                    {
                        if (item.Success)
                        {
                            list.Add(item.Groups["name"].Value.Trim());
                        }
                    }
                }

                return list;
            }, cancellationToken);
        }

        /// <summary>
        /// 获取风格列表
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public async Task<List<string>> GetGenresAsync(
            string html,
            CancellationToken cancellationToken)
        {
            var list = new List<string>();
            return await Task.Run(() =>
            {
                var match = Regex.Match(html, Plugin.Instance.Configuration.GenreListPattern); //@"类别:</p>\s*<p>(?<genres>.*?)\s*</p>"
                if (match.Success)
                {
                    var items = Regex.Matches(match.Groups["genres"].Value, Plugin.Instance.Configuration.GenrePattern); //"href=\"(?<url>.*?)\">(?<name>.*?)<"
                    foreach (Match item in items)
                    {
                        if (item.Success)
                        {
                            list.Add(item.Groups["name"].Value.Trim());
                        }
                    }
                }

                return list;
            }, cancellationToken);
        }

        /// <summary>
        /// 获取演员列表
        /// </summary>
        /// <param name="html">html 内容</param>
        /// <returns></returns>
        public async Task<List<PersonModel>> GetActressesAsync(
            string html,
            CancellationToken cancellationToken)
        {
            var list = new List<PersonModel>();
            return await Task.Run(() =>
            {
                var match = Regex.Match(html, Plugin.Instance.Configuration.ActressListPattern); //@"avatar-waterfall"">\s*(?<actresses>[\w\W]*?)\s*</div>\s*<div"
                if (match.Success)
                {
                    var items = Regex.Matches(match.Groups["actresses"].Value, Plugin.Instance.Configuration.ActressPattern); //@"href=""(?<url>.*?)""[\w\W]*?src=""(?<photo>.*?)""[\w\W]*?<span>(?<name>.*?)<"
                    foreach (Match item in items)
                    {
                        if (item.Success)
                        {
                            list.Add(new PersonModel
                            {
                                Name = item.Groups["name"].Value.Trim(),
                                Avatars = new Avatar { Large = item.Groups["photo"].Value.Trim() },
                                Alt = item.Groups["url"].Value.Trim()
                            });
                        }
                    }
                }

                return list;
            }, cancellationToken);
        }


        public async Task<List<RemoteImageInfo>> GetScreenshotsAsync(
            string html,
            string providerName,
            CancellationToken cancellationToken)
        {
            var list = new List<RemoteImageInfo>();
            return await Task.Run(() =>
            {
                var match = Regex.Match(html, Plugin.Instance.Configuration.ScreenshotListPattern); //sample-waterfall"">\s*(?<thumbnails>[\w\W]*?)\s*</div>\s*<div

                // 匹配到数据
                if (match.Success)
                {
                    // 匹配 预览图 列表
                    var matches = Regex.Matches(match.Groups["thumbnails"].Value, Plugin.Instance.Configuration.ScreenshotPattern); //href="(?<url>.*?)"\stitle="(?<name>.*?)"

                    // 遍历列表
                    foreach (Match m in matches)
                    {
                        // 匹配成功
                        if (m.Success)
                        {
                            // 图片 url
                            var imgUrl = m.Groups["url"].Value.Trim();

                            list.Add(new RemoteImageInfo
                            {
                                ProviderName = providerName,
                                Url = imgUrl,
                                Type = ImageType.Screenshot,
                                ThumbnailUrl = imgUrl.Replace("jp", "") // 缩略文件名
                            });
                        }
                    }
                }
                return list;
            }, cancellationToken);
        }



        /// <summary>
        /// 转换 Dto 到 人员列表
        /// </summary>
        /// <param name="people"></param>
        /// <param name="personType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<PersonInfo>> TransPersonInfoAsync(
            List<PersonModel> people,
            string personType,
            CancellationToken cancellationToken)
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
                    var releaseDate = detail.ReleaseDate?.ToUniversalTime();

                    // 设置 发行日期
                    movie.PremiereDate = releaseDate;
                    // 设置 年份
                    movie.ProductionYear = releaseDate?.Year;
                }

                // 添加类别
                detail.Genres.ForEach((item) =>
                {
                    movie.AddGenre(item);
                });

                // 添加 工作室
                detail.Studios.ForEach((studio) =>
                {
                    movie.AddStudio(studio);
                });

                // 添加 发行商
                detail.Labels.ForEach((label) =>
                {
                    // 不存在时才添加
                    if (!movie.Studios.Contains(label))
                    {
                        movie.AddStudio(label);
                    }
                });

                return movie;

            }, cancellationToken);
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
            var movie = await GetMovieDetailAsync(id, cancellationToken);

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
    }
}
