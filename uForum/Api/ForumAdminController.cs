using System.Linq;
using System.Web;
using System.Web.Http;
using umbraco.cms.businesslogic.web;
using umbraco.NodeFactory;
using Umbraco.Web.WebApi;

namespace uForum.Api
{
    public class ForumAdminController : UmbracoApiController
    {        
        private const string ModeratorRoles = "admin,HQ,Core,MVP";

        [HttpGet]
        [MemberAuthorize(AllowGroup = "admin")]
        public string DeleteTopic(int topicId)
        {
            var topic = Businesslogic.Topic.GetTopic(topicId);
            topic.Delete();

            return "true";
        }

        [HttpGet]
        [MemberAuthorize(AllowGroup = ModeratorRoles)]
        public string MarkTopicAsSpam(int topicId)
        {
            var topic = Businesslogic.Topic.GetTopic(topicId);
            topic.MarkAsSpam();

            return "true";
        }

        [HttpGet]
        [MemberAuthorize(AllowGroup = ModeratorRoles)]
        public string MarkTopicAsHam(int topicId)
        {
            var topic = Businesslogic.Topic.GetTopic(topicId);
            topic.MarkAsHam();

            return "true";
        }

        [HttpGet]
        [MemberAuthorize(AllowGroup = "admin")]
        public string MoveTopic(int topicId, int newForumId)
        {
            var topic = Businesslogic.Topic.GetTopic(topicId);
            topic.Move(newForumId);

            return Xslt.NiceTopicUrl(topic.Id);
        }

        [HttpGet]
        [MemberAuthorize(AllowGroup = "admin")]
        public string DeleteComment(int commentId)
        {
            var comment = new Businesslogic.Comment(commentId);
            comment.Delete();

            return "true";
        }

        [HttpGet]
        [MemberAuthorize(AllowGroup = ModeratorRoles)]
        public string MarkCommentAsSpam(int commentId)
        {
            var comment = new Businesslogic.Comment(commentId);
            comment.MarkAsSpam();

            return "true";
        }

        [HttpGet]
        [MemberAuthorize(AllowGroup = ModeratorRoles)]
        public string MarkCommentAsHam(int commentId)
        {
            var comment = new Businesslogic.Comment(commentId);
            comment.MarkAsHam();

            return "true";
        }
    }
}
