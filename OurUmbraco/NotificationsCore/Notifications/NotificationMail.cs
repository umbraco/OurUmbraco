using System;
using System.Net.Mail;
using System.Xml;
using OurUmbraco.NotificationsWeb;
using Umbraco.Core.Logging;

namespace OurUmbraco.NotificationsCore.Notifications
{
    public class NotificationMail
    {
        public MailAddress FromMailAddress;
        public string Subject;
        public string Domain;
        public string Body;
        
        public NotificationMail GetNotificationMail(string notificationType)
        {
            try
            {
                var notifications = new XmlDocument();
                notifications.Load(Config.ConfigurationFile);

                var settings = notifications.SelectSingleNode("//global");

                var node = notifications.SelectSingleNode(string.Format("//instant//notification [@name = '{0}']", notificationType));

                var details = new XmlDocument();
                var cont = details.CreateElement("details");
                cont.AppendChild(details.ImportNode(settings, true));
                cont.AppendChild(details.ImportNode(node, true));

                var detailsChild = details.AppendChild(cont);

                var fromMail = detailsChild.SelectSingleNode("//from/email").InnerText;
                var fromName = detailsChild.SelectSingleNode("//from/name").InnerText;
                var notificationMail = new NotificationMail
                {
                    FromMailAddress = new MailAddress(fromMail, fromName),
                    Subject = detailsChild.SelectSingleNode("//subject").InnerText,
                    Domain = detailsChild.SelectSingleNode("//domain").InnerText,
                    Body = detailsChild.SelectSingleNode("//body").InnerText
                };

                return notificationMail;
            }
            catch (Exception e)
            {
                LogHelper.Error<MarkAsSolutionReminder>(string.Format("Couldn't get settings for {0}", notificationType), e);
                throw;
            }
        }
    }
}
