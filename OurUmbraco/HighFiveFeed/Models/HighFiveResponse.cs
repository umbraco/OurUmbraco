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
        public string From{ get; set; }
        public string To { get; set; }
        public string FromAvatarUrl { get; set; }
        public string ToAvatarUrl { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LinkTitle { get; set; }

    }
}
