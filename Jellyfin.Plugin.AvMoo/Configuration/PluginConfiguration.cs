using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.AvMoo.Configuration
{
    public enum Language
    {
        /// <summary>
        /// 英语
        /// </summary>
        En,

        /// <summary>
        /// 日语
        /// </summary>
        Ja,

        /// <summary>
        /// 繁体中文
        /// </summary>
        Tw,

        /// <summary>
        /// 简体中文
        /// </summary>
        Cn
    }

    public class PluginConfiguration : BasePluginConfiguration
    {
        // store configurable settings your plugin might need

        /// <summary>
        /// 域名
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 语言
        /// </summary>
        public Language Language { get; set; }

        /// <summary>
        /// 搜索结果正则
        /// </summary>
        public string SearchResultPattern { get; set; }

        /// <summary>
        /// 大封面图正则
        /// </summary>
        public string CoverPattern { get; set; }

        /// <summary>
        /// 番号和标题正则
        /// </summary>
        public string TitlePattern { get; set; }

        /// <summary>
        /// 发行日期正则
        /// </summary>
        public string ReleaseDatePattern { get; set; }

        /// <summary>
        /// 时长正则
        /// </summary>
        public string DurationPattern { get; set; }

        /// <summary>
        /// 导演列表正则
        /// </summary>
        public string DirectorListPattern { get; set; }

        /// <summary>
        /// 导演正则
        /// </summary>
        public string DirectorPattern { get; set; }

        /// <summary>
        /// 工作室列表正则
        /// </summary>
        public string StudioListPattern { get; set; }

        /// <summary>
        /// 工作室正则
        /// </summary>
        public string StudioPattern { get; set; }

        /// <summary>
        /// 发行商列表正则
        /// </summary>
        public string LabelListPattern { get; set; }

        /// <summary>
        /// 发行商正则
        /// </summary>
        public string LabelPattern { get; set; }

        /// <summary>
        /// 系列列表正则
        /// </summary>
        public string CollectionListPattern { get; set; }

        /// <summary>
        /// 系列正则
        /// </summary>
        public string CollectionPattern { get; set; }

        /// <summary>
        /// 类别列表正则
        /// </summary>
        public string GenreListPattern { get; set; }

        /// <summary>
        /// 类别正则
        /// </summary>
        public string GenrePattern { get; set; }

        /// <summary>
        /// 演员列表正则
        /// </summary>
        public string ActressListPattern { get; set; }

        /// <summary>
        /// 演员正则
        /// </summary>
        public string ActressPattern { get; set; }

        /// <summary>
        /// 缩略图列表正则
        /// </summary>
        public string ScreenshotListPattern { get; set; }

        /// <summary>
        /// 缩略图正则
        /// </summary>
        public string ScreenshotPattern { get; set; }

        public PluginConfiguration()
        {
            // set default options here
            Language = Language.Cn;
            Domain = "avmoo.casa";
            // SearchResultPattern = @"/movie/([\d\w]+)";
            SearchResultPattern = @"movie/(?<id>.*?)""[\w\W]*?src=""(?<poster>.*?)""\stitle=""(?<title>.*?)""[\w\W]*?<br><date>(?<avid>.*?)</date>\s/\s<date>(?<date>.*?)</"; 
            CoverPattern = @"bigImage""\shref=""(?<large>.*?)""";
            TitlePattern = @"<h3>(?<id>[A-Z\d\-]+)\s(?<title>.*?)</h3>";
            ReleaseDatePattern = @"发行时间:</span>\s(?<date>.*?)</p>";
            DurationPattern = @"长度:</span>\s(?<duration>\d+)";
            DirectorListPattern = @"导演:</span>\s(?<directors>.*?)</p>";
            DirectorPattern = "href=\"(?<url>.*?)\">(?<name>.*?)<";
            StudioListPattern = @"制作商:\s</p>\s*<p>(?<productors>.*?)\s*</p>";
            StudioPattern = "href=\"(?<url>.*?)\">(?<name>.*?)<";
            LabelListPattern = @"发行商:\s</p>\s*<p>(?<publishers>.*?)\s*</p>";
            LabelPattern = "href=\"(?<url>.*?)\">(?<name>.*?)<";
            CollectionListPattern = @"系列:</p>\s*<p>(?<series>.*?)\s*</p>";
            CollectionPattern = "href=\"(?<url>.*?)\">(?<name>.*?)<";
            GenreListPattern = @"类别:</p>\s*<p>(?<genres>.*?)\s*</p>";
            GenrePattern = "href=\"(?<url>.*?)\">(?<name>.*?)<";
            ActressListPattern = @"avatar-waterfall"">\s*(?<actresses>[\w\W]*?)\s*</div>\s*<div";
            ActressPattern = @"href=""(?<url>.*?)""[\w\W]*?src=""(?<photo>.*?)""[\w\W]*?<span>(?<name>.*?)<";
            ScreenshotListPattern = @"sample-waterfall"">\s*(?<thumbnails>[\w\W]*?)\s*</div>\s*<div";
            ScreenshotPattern = @"href=""(?<url>.*?)""\stitle=""(?<name>.*?)""";
        }
    }
}
