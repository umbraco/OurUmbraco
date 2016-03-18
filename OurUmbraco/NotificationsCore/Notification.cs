using System.Xml;
using OurUmbraco.NotificationsCore.Interfaces;

namespace OurUmbraco.NotificationsCore
{
    public abstract class Notification: INotification
    {
        public virtual bool SendNotification(XmlNode details, params object[] args) { return false; }
    }
}
