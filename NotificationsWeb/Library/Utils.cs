using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using System.Xml;
using NotificationsWeb.BusinessLogic;

namespace NotificationsWeb.Library
{
   
    public class Utils
    {
        public static bool IsSubscribedToForum(int forumId, int memberId)
        {
            return (BusinessLogic.Data.SqlHelper.ExecuteScalar<int>("SELECT 1 FROM forumSubscribers WHERE forumId = @forumId and memberId = @memberId",
                BusinessLogic.Data.SqlHelper.CreateParameter("@forumId", forumId),
                BusinessLogic.Data.SqlHelper.CreateParameter("@memberId", memberId)) > 0);
        }

        public static bool IsSubscribedToForumTopic(int topicId, int memberId)
        {
            return (BusinessLogic.Data.SqlHelper.ExecuteScalar<int>("SELECT 1 FROM forumTopicSubscribers WHERE topicId = @topicId and memberId = @memberId",
                BusinessLogic.Data.SqlHelper.CreateParameter("@topicId", topicId),
                BusinessLogic.Data.SqlHelper.CreateParameter("@memberId", memberId)) > 0);
        }

    }
}
