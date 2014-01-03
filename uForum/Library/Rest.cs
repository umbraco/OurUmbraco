using System.Web;
using umbraco.BusinessLogic;
using umbraco.NodeFactory;
using umbraco.cms.businesslogic.web;
using System.Web.Security;

namespace uForum.Library
{
    public class Rest
    {
        public static string HasAccess(int forumId)
        {
            var retval = "Not Logged On";

            var currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (currentMember > 0)
            {
                retval = "IsLoggedOn";

                var member = Membership.GetUser(currentMember);
                retval += " AS " + member.Email;
            }

            if (Access.HasAccces(forumId, currentMember))
                retval += " and HasAccess";

            return retval;
        }

        public static string EditTopic(int topicId)
        {
            var currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;
            var topic = Businesslogic.Topic.GetTopic(topicId);

            if (topic.Editable(currentMember))
            {
                var title = HttpContext.Current.Request["title"];
                var body = HttpContext.Current.Request["body"];
                var tags = HttpContext.Current.Request["tags"];
                topic.Body = body;
                topic.Title = title;
                //topic.Tags = tags;
                topic.Save(false);

                return Xslt.NiceTopicUrl(topic.Id);
            }
            
            return "0";
        }

        public static string TopicUrl(int topicId)
        {
            HttpContext.Current.Response.Redirect(Xslt.NiceTopicUrl(topicId));
            HttpContext.Current.Response.End();
            return "";
        }

        public static string NewTopic(int forumId)
        {
            var node = new Node(forumId);
            var currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;
           
            if (currentMember > 0 && Access.HasAccces(node.Id, currentMember))
            {
                var title = HttpContext.Current.Request["title"];
                var body = HttpContext.Current.Request["body"];
                var tags = HttpContext.Current.Request["tags"];

                var topic = Businesslogic.Topic.Create(forumId, title, body, currentMember);

                return Xslt.NiceTopicUrl(topic.Id);
            }

            return "0";
        }

        public static string NewComment(int topicId, int itemsPerPage)
        {
            var currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (currentMember > 0 && topicId > 0)
            {
                var body = HttpContext.Current.Request["body"];
                var comment = Businesslogic.Comment.Create(topicId, body, currentMember);

                return Xslt.NiceCommentUrl(comment.TopicId, comment.Id, itemsPerPage);
            }

            return "";
        }

        public static string EditComment(int commentId, int itemsPerPage)
        {
            var currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;
            var comment = new Businesslogic.Comment(commentId);

            if (comment.Editable(currentMember))
            {
                var body = HttpContext.Current.Request["body"];
                comment.Body = body;
                comment.Save();

                return Xslt.NiceCommentUrl(comment.TopicId, comment.Id, itemsPerPage);
            }

            return "";
        }

        public static string DeleteTopic(int topicId)
        {
            var currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (Xslt.IsMemberInGroup("admin", currentMember))
            {
                var topic = Businesslogic.Topic.GetTopic(topicId);
                topic.Delete();

                return "true";
            }

            return "false";
        }

        public static string MarkTopicAsSpam(int topicId)
        {
            if (Utills.IsModerator() == false)
                return "false";

            var topic = Businesslogic.Topic.GetTopic(topicId);
            topic.MarkAsSpam();

            return "true";
        }

        public static string MarkTopicAsHam(int topicId)
        {
            if (Utills.IsModerator() == false) 
                return "false";

            var topic = Businesslogic.Topic.GetTopic(topicId);
            topic.MarkAsHam();

            return "true";
        }

        public static string MoveTopic(int topicId, int newForumId)
        {
            var currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (Xslt.IsMemberInGroup("admin", currentMember))
            {
                var topic = Businesslogic.Topic.GetTopic(topicId);
                topic.Move(newForumId);

                return Xslt.NiceTopicUrl(topic.Id);
            }

            return "false";
        }


        public static string DeleteComment(int commentId)
        {
            var currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (User.GetCurrent() != null || Xslt.IsMemberInGroup("admin", currentMember))
            {
                var comment = new Businesslogic.Comment(commentId);
                comment.Delete();

                return "true";
            }

            return "false";
        }

        public static string MarkCommentAsSpam(int commentId)
        {
            if (Utills.IsModerator() == false) 
                return "false";

            var comment = new Businesslogic.Comment(commentId);
            comment.MarkAsSpam();

            return "true";
        }

        public static string MarkCommentAsHam(int commentId)
        {
            if (Utills.IsModerator() == false) 
                return "false";

            var comment = new Businesslogic.Comment(commentId);
            comment.MarkAsHam();

            return "true";
        }
    }
}
