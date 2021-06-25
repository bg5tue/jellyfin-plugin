using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.AvMoo.Configuration
{
    public enum Language
    {
        /// <summary>
        /// Ӣ��
        /// </summary>
        En,

        /// <summary>
        /// ����
        /// </summary>
        Ja,

        /// <summary>
        /// ��������
        /// </summary>
        Tw,

        /// <summary>
        /// ��������
        /// </summary>
        Cn
    }

    public class PluginConfiguration : BasePluginConfiguration
    {
        // store configurable settings your plugin might need

        /// <summary>
        /// ����
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public Language Language { get; set; }

        /// <summary>
        /// �����������
        /// </summary>
        public string SearchResultPattern { get; set; }

        /// <summary>
        /// �����ͼ����
        /// </summary>
        public string CoverPattern { get; set; }

        /// <summary>
        /// ���źͱ�������
        /// </summary>
        public string TitlePattern { get; set; }

        /// <summary>
        /// ������������
        /// </summary>
        public string ReleaseDatePattern { get; set; }

        /// <summary>
        /// ʱ������
        /// </summary>
        public string DurationPattern { get; set; }

        /// <summary>
        /// �����б�����
        /// </summary>
        public string DirectorListPattern { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public string DirectorPattern { get; set; }

        /// <summary>
        /// �������б�����
        /// </summary>
        public string StudioListPattern { get; set; }

        /// <summary>
        /// ����������
        /// </summary>
        public string StudioPattern { get; set; }

        /// <summary>
        /// �������б�����
        /// </summary>
        public string LabelListPattern { get; set; }

        /// <summary>
        /// ����������
        /// </summary>
        public string LabelPattern { get; set; }

        /// <summary>
        /// ϵ���б�����
        /// </summary>
        public string CollectionListPattern { get; set; }

        /// <summary>
        /// ϵ������
        /// </summary>
        public string CollectionPattern { get; set; }

        /// <summary>
        /// ����б�����
        /// </summary>
        public string GenreListPattern { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        public string GenrePattern { get; set; }

        /// <summary>
        /// ��Ա�б�����
        /// </summary>
        public string ActressListPattern { get; set; }

        /// <summary>
        /// ��Ա����
        /// </summary>
        public string ActressPattern { get; set; }

        /// <summary>
        /// ����ͼ�б�����
        /// </summary>
        public string ScreenshotListPattern { get; set; }

        /// <summary>
        /// ����ͼ����
        /// </summary>
        public string ScreenshotPattern { get; set; }

        public PluginConfiguration()
        {
            // set default options here
            Language = Language.Cn;
            Domain = "avmoo.casa";
            SearchResultPattern = @"/movie/([\d\w]+)";
            CoverPattern = @"bigImage""\shref=""(?<large>.*?)""";
            TitlePattern = @"<h3>(?<id>[A-Z\d\-]+)\s(?<title>.*?)</h3>";
            ReleaseDatePattern = @"����ʱ��:</span>\s(?<date>.*?)</p>";
            DurationPattern = @"����:</span>\s(?<duration>\d+)";
            DirectorListPattern = @"����:</span>\s(?<directors>.*?)</p>";
            DirectorPattern = "href=\"(?<url>.*?)\">(?<name>.*?)<";
            StudioListPattern = @"������:\s</p>\s*<p>(?<productors>.*?)\s*</p>";
            StudioPattern = "href=\"(?<url>.*?)\">(?<name>.*?)<";
            LabelListPattern = @"������:\s</p>\s*<p>(?<publishers>.*?)\s*</p>";
            LabelPattern = "href=\"(?<url>.*?)\">(?<name>.*?)<";
            CollectionListPattern = @"ϵ��:</p>\s*<p>(?<series>.*?)\s*</p>";
            CollectionPattern = "href=\"(?<url>.*?)\">(?<name>.*?)<";
            GenreListPattern = @"���:</p>\s*<p>(?<genres>.*?)\s*</p>";
            GenrePattern = "href=\"(?<url>.*?)\">(?<name>.*?)<";
            ActressListPattern = @"avatar-waterfall"">\s*(?<actresses>[\w\W]*?)\s*</div>\s*<div";
            ActressPattern = @"href=""(?<url>.*?)""[\w\W]*?src=""(?<photo>.*?)""[\w\W]*?<span>(?<name>.*?)<";
            ScreenshotListPattern = @"sample-waterfall"">\s*(?<thumbnails>[\w\W]*?)\s*</div>\s*<div";
            ScreenshotPattern = "href=\"(?<url>.*?)\">(?<name>.*?)<";
        }
    }
}
