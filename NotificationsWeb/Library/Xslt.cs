using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using System.Xml;
using NotificationsWeb.BusinessLogic;

namespace NotificationsWeb.Library
{
    [Umbraco.Core.Macros.XsltExtension("Notifications")]
    public class Xslt
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

        public static XPathNodeIterator SubscribedForums(int memberId)
        {
            XmlDocument xd = new XmlDocument();
            XmlNode x = xd.CreateElement("forums");

            List<uForum.Businesslogic.Forum> forums = ForumTopic.GetSubscribedForums(memberId);
            foreach (uForum.Businesslogic.Forum f in forums)
            {
                x.AppendChild(f.ToXml(xd,false));
            }

            return x.CreateNavigator().Select(".");
        }

        public static XPathNodeIterator SubscribedTopics(int memberId)
        {
            XmlDocument xd = new XmlDocument();
            XmlNode x = xd.CreateElement("topics");

            List<uForum.Businesslogic.Topic> topics = ForumTopic.GetSubscribedTopics(memberId);
            foreach (uForum.Businesslogic.Topic t in topics)
            {
                x.AppendChild(t.ToXml(xd));
            }

            return x.CreateNavigator().Select(".");
        }
    }
}
