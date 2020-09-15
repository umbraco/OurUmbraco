using System.Collections.Generic;

namespace OurUmbraco.Community.Banner.Models
{
    public class BannersViewModel
    {
        public IEnumerable<Banner> Collection { get; internal set; }

        public Location.Models.Location Location { get; internal set; }
    }
}
