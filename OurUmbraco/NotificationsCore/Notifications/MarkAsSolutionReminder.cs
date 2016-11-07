using System;
using System.IO;
using System.Net.Mail;
using System.Web.Hosting;
using System.Xml;
using OurUmbraco.Forum.Services;
using OurUmbraco.NotificationsWeb;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace OurUmbraco.NotificationsCore.Notifications
{
    public class MarkAsSolutionReminder
    {
        private readonly XmlNode _details;

        public MarkAsSolutionReminder()
        {
            const string notificationName = "MarkAsSolutionReminderSingle";
            try
            {
                var notifications = new XmlDocument();
                notifications.Load(Config.ConfigurationFile);

                var settings = notifications.SelectSingleNode("//global");

                var node = notifications.SelectSingleNode(string.Format("//instant//notification [@name = '{0}']", notificationName));

                var details = new XmlDocument();
                var cont = details.CreateElement("details");
                cont.AppendChild(details.ImportNode(settings, true));
                cont.AppendChild(details.ImportNode(node, true));

                _details = details.AppendChild(cont);
            }
            catch (Exception e)
            {
                LogHelper.Error<MarkAsSolutionReminder>(string.Format("Couldn't get settings for {0}", notificationName), e);
                throw;
            }
        }

        public void SendNotification(int topicId, int memberId)
        {
            try
            {
                var topicService = new TopicService(ApplicationContext.Current.DatabaseContext);
                var topic = topicService.GetById(topicId);

                using (ContextHelper.EnsureHttpContext())
                {
                    var memberShipHelper = new MembershipHelper(UmbracoContext.Current);
                    var member = memberShipHelper.GetById(memberId);
                    
                    using (var smtpClient = new SmtpClient())
                    {
                        var fromEmail = _details.SelectSingleNode("//from/email").InnerText;
                        var fromName = _details.SelectSingleNode("//from/name").InnerText;
                        var fromMailAddress = new MailAddress(fromEmail, fromName);
                        
                        var subject = string.Format("{0} - '{1}'", _details.SelectSingleNode("//subject").InnerText, topic.Title);
                        var domain = _details.SelectSingleNode("//domain").InnerText;
                        var body = _details.SelectSingleNode("//body").InnerText;
                        body = string.Format(body, topic.Title, "https://" + domain + topic.GetUrl());

                        if (member.GetPropertyValue<bool>("bugMeNot") == false)
                        {
                            const string notificationTestFolder = "~/App_Data/NotificationTest";
                            if (Directory.Exists(HostingEnvironment.MapPath(notificationTestFolder)) == false)
                                Directory.CreateDirectory(HostingEnvironment.MapPath(notificationTestFolder));

                            File.AppendAllText(string.Format("{0}/{1}.txt", HostingEnvironment.MapPath(notificationTestFolder), topic.Id), 
                                string.Format("To: {0}{3}Subject: {1}{3}Body: {3}{2}", member.GetPropertyValue<string>("Email"), subject, body, Environment.NewLine));

                            using (var mailMessage = new MailMessage())
                            {
                                mailMessage.Subject = subject;
                                mailMessage.Body = body;

                                mailMessage.To.Add(member.GetPropertyValue<string>("Email"));
                                mailMessage.From = fromMailAddress;

                                smtpClient.Send(mailMessage);
                            }
                        }
                    }
                }

                var db = ApplicationContext.Current.DatabaseContext.Database;
                var sql = new Sql("UPDATE forumTopics SET markAsSolutionReminderSent = 1 WHERE id = @topicId", new { topicId = topic.Id });
                var result = db.Execute(sql);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MarkAsSolutionReminder>("Error processing notification", ex);
                throw;
            }
        }
    }
}
