using System;
using System.Collections.Generic;
using System.Web;
using umbraco.BusinessLogic;
using umbraco.presentation.umbracobase.library;
using umbraco.cms.businesslogic.web;
using System.Web.Security;

namespace uForum.Library {
    public class Rest {
        
        //public int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;
        public static string HasAccess(int forumId) {
            string retval = "Not Logged On";

            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (_currentMember > 0){
                retval = "IsLoggedOn";

            MembershipUser member = Membership.GetUser(_currentMember);
            retval += " AS " + member.Email;
            }

            if (Access.HasAccces(forumId, _currentMember))
                retval += " and HasAccess";

            return retval;
        }

        public static string EditTopic(int topicId) {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;
            Businesslogic.Topic t = new uForum.Businesslogic.Topic(topicId);

            if (t.Editable(_currentMember))
            {
                string title = HttpContext.Current.Request["title"];
                string body = HttpContext.Current.Request["body"];

                t.Body = body;
                t.Title = title;
                t.Save(false);
                
                return Library.Xslt.NiceTopicUrl(t.Id);
            } else {
                return "0";
            }
        }

        public static string TopicUrl(int topicId)
        {
            return Library.Xslt.NiceTopicUrl(topicId);
        }

        public static string NewTopic(int forumId) {
            umbraco.presentation.nodeFactory.Node n = new umbraco.presentation.nodeFactory.Node(forumId);
            int _currentMember =
                HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;


            if (_currentMember > 0 && n != null && Access.HasAccces(n.Id, _currentMember)) {
                string title = HttpContext.Current.Request["title"];
                string body = HttpContext.Current.Request["body"];

                Businesslogic.Topic t = Businesslogic.Topic.Create(forumId, title, body, _currentMember);

                return Library.Xslt.NiceTopicUrl(t.Id);
            } else {
                return "0";
            }
        }

        public static string NewComment(int topicId, int itemsPerPage) {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (_currentMember > 0 && topicId > 0) {
                string body = HttpContext.Current.Request["body"];
                Businesslogic.Comment c = Businesslogic.Comment.Create(topicId, body, _currentMember);

                return Xslt.NiceCommentUrl(c.TopicId, c.Id, itemsPerPage);
            }

            return "";
        }

        public static string EditComment(int commentId, int itemsPerPage)
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;
            Businesslogic.Comment c = new uForum.Businesslogic.Comment(commentId);

            if (c.Editable(_currentMember))
            {

                string body = HttpContext.Current.Request["body"];
                c.Body = body;
                c.Save();

                return Xslt.NiceCommentUrl(c.TopicId, c.Id, itemsPerPage);
            }

            return "";
        }

        public static string DeleteTopic(int topicId) {

            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (Xslt.IsMemberInGroup("admin", _currentMember)) {
                
                Businesslogic.Topic t = new Businesslogic.Topic(topicId);
                t.Delete();

                return "true";
            }

            return "false";
        }

        public static string MoveTopic(int topicId, int NewForumId) {

            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (Xslt.IsMemberInGroup("admin", _currentMember)) {

                Businesslogic.Topic t = new Businesslogic.Topic(topicId);
                t.Move(NewForumId);
                
                return Xslt.NiceTopicUrl(t.Id);
            }

            return "false";
        }


        public static string DeleteComment(int commentId) {

            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (User.GetCurrent() != null || Xslt.IsMemberInGroup("admin", _currentMember)) 
            {
                Businesslogic.Comment c = new uForum.Businesslogic.Comment(commentId);
                c.Delete();
                
                return "true";
            }

            return "false";
        }

    }
}
