using System.Linq;
using System.Web;
using System.Web.Http;
using uForum.Library;
using umbraco.cms.businesslogic.web;
using umbraco.NodeFactory;
using Umbraco.Web.WebApi;

namespace uForum.Api
{
    public class ForumController : UmbracoApiController
    {        
        private const string ModeratorRoles = "admin,HQ,Core,MVP";

        [HttpGet]
        public string TopicUrl(int topicId)
        {
            HttpContext.Current.Response.Redirect(Xslt.NiceTopicUrl(topicId));
            HttpContext.Current.Response.End();
            return "";
        }

        [HttpPost]
        public string NewTopic(int forumId)
        {
            var node = new Node(forumId);

            if (Members.GetCurrentMember().Id > 0 && Access.HasAccces(node.Id, Members.GetCurrentMember().Id))
            {
                var title = HttpContext.Current.Request["title"];
                var body = HttpContext.Current.Request["body"];
                var tags = HttpContext.Current.Request["tags"];

                var topic = Businesslogic.Topic.Create(forumId, title, body, Members.GetCurrentMember().Id);

                return Xslt.NiceTopicUrl(topic.Id);
            }

            return "0";
        }

        [HttpPost]
        public string EditTopic(int topicId)
        {
            var topic = Businesslogic.Topic.GetTopic(topicId);

            if (topic.Editable(Members.GetCurrentMember().Id) == false)
                return "0";

            var title = HttpContext.Current.Request["title"];
            var body = HttpContext.Current.Request["body"];
            var tags = HttpContext.Current.Request["tags"];
            topic.Body = body;
            topic.Title = title;
            //topic.Tags = tags;
            topic.Save(false);

            return Xslt.NiceTopicUrl(topic.Id);
        }

        [HttpPost]
        public string NewComment(int topicId, int itemsPerPage)
        {
            if (Members.GetCurrentMember().Id > 0 && topicId > 0)
            {
                var body = HttpContext.Current.Request["body"];
                var comment = Businesslogic.Comment.Create(topicId, body, Members.GetCurrentMember().Id);

                return Xslt.NiceCommentUrl(comment.TopicId, comment.Id, itemsPerPage);
            }

            return "";
        }

        [HttpPost]
        public string EditComment(int commentId, int itemsPerPage)
        {
            var comment = new Businesslogic.Comment(commentId);

            if (comment.Editable(Members.GetCurrentMember().Id))
            {
                var body = HttpContext.Current.Request["body"];
                comment.Body = body;
                comment.Save();

                return Xslt.NiceCommentUrl(comment.TopicId, comment.Id, itemsPerPage);
            }

            return "";
        }

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
