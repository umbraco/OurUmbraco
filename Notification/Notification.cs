using System;
using System.Collections.Generic;
using System.Text;
using NotificationsCore.Interfaces;
using System.Xml;

namespace NotificationsCore
{
    public abstract class Notification: INotification
    {
        #region INotification Members

        public virtual bool SendNotification(XmlNode details, params object[] args) { return false; }

        #endregion
    }
}
