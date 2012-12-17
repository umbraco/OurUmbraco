using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace NotificationsWeb.Library
{
    public class Rest
    {
        public static string SubscribeToForumTopic(int topicId)
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (_currentMember > 0)
            {
                BusinessLogic.ForumTopic.Subscribe(topicId, _currentMember);

                return "true";
            }

            return "false";
        }


        public static string UnSubscribeFromForumTopic(int topicId)
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (_currentMember > 0)
            {
                BusinessLogic.ForumTopic.UnSubscribe(topicId, _currentMember);

                return "true";
            }

            return "false";
        }

        public static string SubscribeToForum(int forumId)
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (_currentMember > 0)
            {
                BusinessLogic.Forum.Subscribe(forumId, _currentMember);

                return "true";
            }

            return "false";
        }


        public static string UnSubscribeFromForum(int forumId)
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (_currentMember > 0)
            {
                BusinessLogic.Forum.UnSubscribe(forumId, _currentMember);

                return "true";
            }

            return "false";
        }
    }
}
