using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurUmbraco.HighFiveFeed.Models
{
    public class HighFiveResponse
    {
        public int Id { get; set; }
        public string from{ get; set; }
        public string to { get; set; }
        public string fromAvatarUrl { get; set; }
        public string toAvatarUrl { get; set; }
        public string type { get; set; }
        public string url { get; set; }

    }
}
