using Newtonsoft.Json;
using OurUmbraco.Forum.Models;
using OurUmbraco.Forum.Services;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Forum.Controllers
{
    public class DiscourseController : UmbracoApiController
    {
        [HttpPost]
        public IHttpActionResult Post(TopicModel topicModel)
        {
            var topicService = new TopicService(ApplicationContext.DatabaseContext);
            var topic = topicService.GetById(topicModel.TopicId);
            var body = topic.Body.Replace(": /media/upload/", ": https://our.umbraco.com/media/upload/");
            body = body + "\n<hr>\n<small>This is a companion discussion topic for the original entry at <a href=\"https://cultiv.nl/blog/released-search-engine-sitemap-package\">https://cultiv.nl/blog/released-search-engine-sitemap-package</a></small>";
            var discourseCreateTopic = new DiscourseCreateTopic
            {
                Title = topic.Title,
                Raw = body,
                Category = 5,
                CreatedAt = topic.Created,
                EmbedUrl = $"https://our.umbraco.com/forum/using-umbraco/{topic.Id}-{topic.Title.ToUrlSegment()}",
                ExternalId = topic.Id
            };

            var x = discourseCreateTopic;
            return Ok(x);
        }

        public class TopicModel
        {
            public int TopicId { get; set; }
        }

    }
}
