using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.AvMoo.Dtos
{
    /// <summary>
    /// 搜索结果项
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 番号
        /// </summary>
        public string Avid { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 发行日期
        /// </summary>
        public string ReleaseData { get; set; }

        /// <summary>
        /// 封面 URL
        /// </summary>
        public string Poster { get; set; }
    }
}
