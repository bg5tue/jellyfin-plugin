using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.AvSox.Dtos
{
    public class PersonModel
    {
        public string Name { get; set; }
        public string Alt { get; set; }
        public string Id { get; set; }
        public Avatar Avatars { get; set; } = new Avatar { };
    }
}
