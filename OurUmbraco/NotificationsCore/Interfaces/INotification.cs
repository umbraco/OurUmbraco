using System.Xml;

namespace OurUmbraco.NotificationsCore.Interfaces
{
    interface INotification
    {
        bool SendNotification(XmlNode details, params object[] args);
    }
}
