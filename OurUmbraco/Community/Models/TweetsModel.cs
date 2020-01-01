using System.Collections.Generic;
using TweetSharp;

namespace OurUmbraco.Community.Models
{
    public class TweetsModel
    {
        public IEnumerable<TwitterStatus> Tweets { get; set; }
        public bool ShowAdminOverView { get; set; }
    }
}
