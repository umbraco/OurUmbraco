using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using System.Xml;
using NotificationsWeb.Services;
using Umbraco.Core;

namespace NotificationsWeb.Library
{
 
    //used from razor //
    public class Utils
    {
        public static bool IsSubscribedToForum(int forumId, int memberId)
        {
            var ns = new NotificationService(ApplicationContext.Current.DatabaseContext);
            return ns.IsSubscribedToForum(forumId, memberId);
            
        }

        public static bool IsSubscribedToForumTopic(int topicId, int memberId)
        {
            var ns = new NotificationService(ApplicationContext.Current.DatabaseContext);
            return ns.IsSubscribedToTopic(topicId, memberId);
        }

    }
}
