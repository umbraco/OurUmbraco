using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace NotificationsCore.Interfaces
{
    interface INotification
    {
        bool SendNotification(XmlNode details, params object[] args);
    }
}
