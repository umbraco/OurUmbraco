using System;
using OurUmbraco.Forum.Models;
using Umbraco.Core.Logging;

namespace OurUmbraco.NotificationsCore.NotificationTypes
{
    public class NewForumTopic : Notification
    {
        public override bool SendNotification(System.Xml.XmlNode details, params object[] args)
        {
            try
            {
                var topic = (Topic) args[0];
                var url = (string) args[1];
                var memberName = (string) args[2];

                if (topic.IsSpam)
                {
                    LogHelper.Debug<NewForumTopic>(string.Format(
                        "Topic Id {0} is marked as spam, no notification sent", topic.Id));
                    return true;
                }

                var newForumTopicNotification = new Notifications.NewForumTopic();
                newForumTopicNotification.SendNotification(topic, memberName, url);
            }
            catch (Exception exception)
            {
                LogHelper.Error<NewForumTopic>("Error", exception);
            }

            return true;
        }
    }
}
