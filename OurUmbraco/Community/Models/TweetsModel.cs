using Tweetinvi.Models;

namespace OurUmbraco.Community.Models
{
    public class TweetsModel
    {
        public ITweet[] Tweets { get; set; }
        public bool ShowAdminOverView { get; set; }
    }
}
