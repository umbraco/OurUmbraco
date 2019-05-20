using OurUmbraco.Forum.Services;
using OurUmbraco.Our.Services;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Forum.Api
{
    /// <summary>
    /// Base abstract controller that expose the forum services
    /// </summary>
    public abstract class ForumControllerBase : UmbracoApiController
    {
        protected ForumControllerBase()
        {
            TopicService = new TopicService(DatabaseContext);
            CommentService = new CommentService(DatabaseContext, TopicService);
            ForumService = new ForumService(DatabaseContext);
            SlackService = new SlackService();
        }

        protected TopicService TopicService { get; private set; }
        protected CommentService CommentService { get; private set; }
        protected ForumService ForumService { get; private set; }
        protected SlackService SlackService { get; private set; }
    }
}