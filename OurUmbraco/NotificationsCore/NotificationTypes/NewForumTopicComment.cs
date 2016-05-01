using System;
using OurUmbraco.Forum.Models;
using Umbraco.Core.Logging;

namespace OurUmbraco.NotificationsCore.NotificationTypes
{
    public class NewForumTopicComment : Notification
    {
        public override bool SendNotification(System.Xml.XmlNode details, params object[] args)
        {
            try
            {
                var comment = (Comment)args[0];
                var topic = (Topic)args[1];
                var url = (string)args[2];
                var memberName = (string)args[3];

                if (comment.IsSpam)
                {
                    LogHelper.Debug<NewForumTopicComment>(
                        string.Format("Comment Id {0} is marked as spam, no notification sent", comment.Id));
                    return true;
                }
                
                var newForumTopicCommentNotification = new Notifications.NewForumComment();
                newForumTopicCommentNotification.SendNotification(comment, memberName, url);
            }
            catch (Exception exception)
            {
                LogHelper.Error<NewForumTopicComment>("Error", exception);
            }

            return true;
        }
    }
}
