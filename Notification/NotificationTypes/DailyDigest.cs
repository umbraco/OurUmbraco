using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotificationsCore.NotificationTypes
{
    public class DailyDigest: Notification
    {
        public DailyDigest()
        {
        }

        public override bool SendNotification(System.Xml.XmlNode details, params object[] args)
        {
            return true;
        }
    }
}
