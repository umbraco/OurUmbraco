using System;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using OurUmbraco.Forum.Models;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace OurUmbraco.NotificationsCore.Notifications
{
    public class NewForumTopic
    {
        private readonly XmlNode _details;
        
        public NewForumTopic()
        {
            var notifications = new XmlDocument();
            notifications.Load(HostingEnvironment.MapPath("~/Config/Notification.config"));

            var settings = notifications.SelectSingleNode("//global");

            var node = notifications.SelectSingleNode("//instant//notification [@name = 'NewTopic']");

            var details = new XmlDocument();
            var cont = details.CreateElement("details");

            cont.AppendChild(details.ImportNode(settings, true));
            cont.AppendChild(details.ImportNode(node, true));

            _details = details.AppendChild(cont);
        }

        public void SendNotification(Topic topic, string memberName, string url)
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;
            var sql = new Sql().Select("memberId")
                .From("forumSubscribers")
                .Where("forumId = @forumId", new { forumId = topic.ParentId });
            var results = db.Query<int>(sql).ToList();

            using (ContextHelper.EnsureHttpContext())
            {
                var memberShipHelper = new MembershipHelper(UmbracoContext.Current);
                {
                    foreach (var memberId in results.Where(memberId => memberId != topic.MemberId))
                    {
                        try
                        {
                            var member = memberShipHelper.GetById(memberId);

                            if (member.GetPropertyValue<bool>("bugMeNot"))
                                continue;

                            var from = new MailAddress(_details.SelectSingleNode("//from/email").InnerText,
                                _details.SelectSingleNode("//from/name").InnerText);

                            var contentService = ApplicationContext.Current.Services.ContentService;
                            var forum = contentService.GetById(topic.ParentId);
                            var subject = _details.SelectSingleNode("//subject").InnerText;
                            subject = string.Format(subject, forum.Name);

                            var domain = _details.SelectSingleNode("//domain").InnerText;

                            var body = _details.SelectSingleNode("//body").InnerText;
                            body = string.Format(body, forum.Name, "https://" + domain + url, memberName, topic.Title,
                                HttpUtility.HtmlDecode(umbraco.library.StripHtml(topic.Body)));

                            var mailMessage = new MailMessage
                            {
                                Subject = subject,
                                Body = body
                            };

                            mailMessage.To.Add(member.GetPropertyValue<string>("Email"));
                            mailMessage.From = from;
                            using (var smtpClient = new SmtpClient())
                            {
                                smtpClient.Send(mailMessage);
                            }
                        }
                        catch (Exception exception)
                        {
                            LogHelper.Error<NotificationTypes.NewForumTopic>(
                                string.Format("Error sending mail to member id {0}", memberId), exception);
                        }
                    }
                }
            }
        }
    }
}