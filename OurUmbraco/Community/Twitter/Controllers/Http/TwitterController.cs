using OurUmbraco.Community.Twitter.Models;
using System.Linq;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Community.Twitter.Controllers.Http
{
    public class TwitterController : UmbracoApiController
    {
        private readonly TwitterService _twitterService;

        public TwitterController()
        {
            _twitterService = new TwitterService();
        }

        public IHttpActionResult GetFilteredTweets()
        {
            var result = _twitterService.GetTweets(30, false);

            var tweets = result.Tweets.Select(x => new TweetDto
            {
                Id = x.Id,
                ScreenName = x.Author.ScreenName,
                ProfileImageUrl = x.Author.ProfileImageUrl.Replace("http://", "https://").Replace("_normal.", "."),
                Text = x.Text,
                CreatedDate = x.CreatedDate
            });


            return Ok(tweets);
        }
    }
}
