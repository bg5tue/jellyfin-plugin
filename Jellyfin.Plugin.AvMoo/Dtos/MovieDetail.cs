using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.AvMoo.Dtos
{
    public class MovieDetail
    {
        /// <summary>
        /// 番号
        /// </summary>
        public string AvId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 发行日期
        /// </summary>
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 时长
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        public string Intro { get; set; }

        public Avatar Images { get; set; } = new Avatar();

        /// <summary>
        /// 大封面
        /// </summary>
        public string Cover { get; set; }

        /// <summary>
        /// 小封面
        /// </summary>
        public string SmallCover => Cover?.Replace("pl", "ps");

        public List<PersonModel> Directors { get; set; } = new List<PersonModel>();

        public List<string> Productors { get; set; } = new List<string>();

        public List<string> Publishers { get; set; } = new List<string>();

        public List<string> Series { get; set; } = new List<string>();

        public List<string> Genres { get; set; } = new List<string>();

        public List<PersonModel> Actresses { get; set; } = new List<PersonModel>();

        public List<string> Thumbnails { get; set; } = new List<string>();

        // season information
        public int? Seasons_Count { get; set; }
        public int? Current_Season { get; set; }
        public int? Episodes_Count { get; set; }

        public string FromUrl { get; set; }
    }
}
