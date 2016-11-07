using System;
using System.IO;
using System.Net.Mail;
using System.Web.Hosting;
using OurUmbraco.Forum.Services;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace OurUmbraco.NotificationsCore.Notifications
{
    public class MarkAsSolutionReminder
    {
        public void SendNotification(int topicId, int memberId, NotificationMail notificationMail)
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
                        var fromMailAddress = notificationMail.FromMailAddress;
                        
                        var subject = string.Format("{0} - '{1}'", notificationMail.Subject, topic.Title);
                        var domain = notificationMail.Domain;
                        var body = notificationMail.Body;
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

                using (var db = ApplicationContext.Current.DatabaseContext.Database)
                {
                    var sql = new Sql("UPDATE forumTopics SET markAsSolutionReminderSent = 1 WHERE id = @topicId", new {topicId = topic.Id});
                    var result = db.Execute(sql);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<MarkAsSolutionReminder>("Error processing notification", ex);
                throw;
            }
        }
    }
}
