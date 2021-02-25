using OurUmbraco.Community.Videos.Models;
using System.Linq;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Community.Videos.Controllers.Http
{
    public class VideoController : UmbracoApiController
    {
        private readonly CommunityVideosService _videoService;

        public VideoController()
        {
            _videoService = new CommunityVideosService();
        }

        public IHttpActionResult GetAll()
        {
            var videos = _videoService.LoadYouTubePlaylistVideos();
            var dtos = videos.Select(x => new YouTubeVideoDto
            {
                Id = x.Id,
                Title = x.Snippet.Title,
                Description = x.Snippet.Description,
                Length = x.ContentDetails.Duration.Value.TotalMinutes,
                PublishedAt = x.Snippet.PublishedAt,
                Likes = x.Statistics.LikeCount,
                Plays = x.Statistics.ViewCount,
                Tags = x.Snippet.Tags,
                ThumbnailUrl = $"https://our.umbraco.com/media/YouTube/{x.Id}.jpg"
            });

            return Ok(dtos);
        }
    }
}
