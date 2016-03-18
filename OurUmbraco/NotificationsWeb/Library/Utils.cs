using OurUmbraco.NotificationsWeb.Services;
using Umbraco.Core;

namespace OurUmbraco.NotificationsWeb.Library
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
