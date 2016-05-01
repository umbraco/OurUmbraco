using System;
using System.Net.Mail;
using System.Web.Hosting;
using System.Xml;
using Umbraco.Core.Logging;

namespace OurUmbraco.NotificationsCore.Notifications
{
    public class AccountApproved
    {
        private readonly XmlNode _details;

        public AccountApproved()
        {
            var notifications = new XmlDocument();
            notifications.Load(HostingEnvironment.MapPath("~/Config/Notification.config"));

            var settings = notifications.SelectSingleNode("//global");

            var node = notifications.SelectSingleNode("//instant//notification [@name = 'AccountApproved']");

            var details = new XmlDocument();
            var cont = details.CreateElement("details");

            cont.AppendChild(details.ImportNode(settings, true));
            cont.AppendChild(details.ImportNode(node, true));

            _details = details.AppendChild(cont);
        }

        public void SendNotification(string memberEmail)
        {
            try
            {
                var from = new MailAddress(_details.SelectSingleNode("//from/email").InnerText,
                    _details.SelectSingleNode("//from/name").InnerText);

                var subject = _details.SelectSingleNode("//subject").InnerText;

                var body = _details.SelectSingleNode("//body").InnerText;

                var mailMessage = new MailMessage
                {
                    Subject = subject,
                    Body = body
                };

                mailMessage.To.Add(memberEmail);
                mailMessage.From = from;
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Send(mailMessage);
                }
            }
            catch (Exception exception)
            {
                LogHelper.Error<AccountApproved>(
                    string.Format("Error sending mail to member {0}", memberEmail), exception);
            }
        }
    }
}