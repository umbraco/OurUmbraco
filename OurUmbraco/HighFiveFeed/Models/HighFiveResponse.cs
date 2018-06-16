using System;

namespace OurUmbraco.HighFiveFeed.Models
{
    public class HighFiveResponse
    {
        public int Id { get; set; }
        public string From{ get; set; }
        public string To { get; set; }
        public string FromAvatarUrl { get; set; }
        public string ToAvatarUrl { get; set; }
        public string Category { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LinkUrl { get; set; }
        public string LinkTitle { get; set; }

    }
}
