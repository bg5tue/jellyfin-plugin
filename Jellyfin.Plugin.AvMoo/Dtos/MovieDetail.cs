using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.AvMoo.Dtos
{
    public class MovieDetail
    {
        public string AvId { get; set; }

        public string Title { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public int Year { get; set; }

        public int Duration { get; set; }

        public string Intro { get; set; }

        public Avatar Images { get; set; } = new Avatar();

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
