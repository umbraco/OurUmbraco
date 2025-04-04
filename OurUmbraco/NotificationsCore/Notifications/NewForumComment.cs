﻿using System;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using OurUmbraco.Forum.Models;
using OurUmbraco.Forum.Services;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace OurUmbraco.NotificationsCore.Notifications
{
    class NewForumComment
    {
        private readonly XmlNode _details;

        public NewForumComment()
        {
            var notifications = new XmlDocument();
            notifications.Load(HostingEnvironment.MapPath("~/Config/Notification.config"));

            var settings = notifications.SelectSingleNode("//global");

            var node = notifications.SelectSingleNode("//instant//notification [@name = 'NewComment']");

            var details = new XmlDocument();
            var cont = details.CreateElement("details");

            cont.AppendChild(details.ImportNode(settings, true));
            cont.AppendChild(details.ImportNode(node, true));

            _details = details.AppendChild(cont);
        }

        public void SendNotification(Comment comment, string memberName, string url)
        {
            var topicService = new TopicService(ApplicationContext.Current.DatabaseContext);
            var topic = topicService.GetById(comment.TopicId);

            var db = ApplicationContext.Current.DatabaseContext.Database;
            var sql = new Sql().Select("memberId")
                .From("forumTopicSubscribers")
                .Where("topicId = @topicId", new { topicId = topic.Id });

            var results = db.Query<int>(sql).ToList();

            using (ContextHelper.EnsureHttpContext())
            {
                var memberShipHelper = new MembershipHelper(UmbracoContext.Current);

                foreach (var memberId in results.Where(memberId => memberId != comment.MemberId))
                {
                    try
                    {
                        var member = memberShipHelper.GetById(memberId);
                        if (member.GetPropertyValue<bool>("bugMeNot"))
                            continue;

                        var from = new MailAddress(_details.SelectSingleNode("//from/email").InnerText,
                            _details.SelectSingleNode("//from/name").InnerText);

                        var subject = string.Format(_details.SelectSingleNode("//subject").InnerText, topic.Title);

                        var domain = _details.SelectSingleNode("//domain").InnerText;
                        var body = _details.SelectSingleNode("//body").InnerText;
                        body = string.Format(body, topic.Title, "https://" + domain + url + "#comment-" + comment.Id, memberName,
                            HttpUtility.HtmlDecode(comment.Body.StripHtml()));

                        var mailMessage = new MailMessage
                        {
                            Subject = subject,
                            Body = body
                        };

                        mailMessage.To.Add(member.GetPropertyValue<string>("Email"));
                        mailMessage.From = @from;

                        using (var smtpClient = new SmtpClient())
                        {
                            smtpClient.Send(mailMessage);
                        }
                    }
                    catch (Exception exception)
                    {
                        LogHelper.Error<NewForumComment>(
                            string.Format("Error sending mail to member id {0}", memberId), exception);
                    }
                }
            }
        }
        
        public void SendNewForumTopicCreatedNotification(int topicId, string newUrl)
        {
            var topicService = new TopicService(ApplicationContext.Current.DatabaseContext);
            var topic = topicService.GetById(topicId);

            var db = ApplicationContext.Current.DatabaseContext.Database;
            var sql = new Sql().Select("memberId")
                .From("forumTopicSubscribers")
                .Where("topicId = @topicId", new { topicId = topic.Id });

            var results = db.Query<int>(sql).ToList();

            using (ContextHelper.EnsureHttpContext())
            {
                var memberShipHelper = new MembershipHelper(UmbracoContext.Current);

                foreach (var memberId in results)
                {
                    try
                    {
                        var member = memberShipHelper.GetById(memberId);
                        if (member.GetPropertyValue<bool>("bugMeNot"))
                            continue;

                        var from = new MailAddress(_details.SelectSingleNode("//from/email").InnerText,
                            _details.SelectSingleNode("//from/name").InnerText);

                        //var subject = string.Format(_details.SelectSingleNode("//subject").InnerText, topic.Title);
                        var subject = $"The forum topic '{topic.Title}' is continued on the new forum";

                        var domain = _details.SelectSingleNode("//domain").InnerText;
                        var discourseBlogPostUrl = System.Configuration.ConfigurationManager.AppSettings["DiscourseBlogPostUrl"];
                        var body = $@"Someone is continuing a topic you were involved in.

--------------------------------------------------

The topic with title '{topic.Title}' has been moved from it's old URL (https://{domain}{topic.GetUrl()}) to the new forum.

You can view the new topic here: 
{newUrl}
      
The forum is moving to a new platform, and we invite you to join us there. You can read all about the new forum in the following blog post: {discourseBlogPostUrl}

Thank You from the Umbraco Community!
--------------------------------------------------
You get this notification because you are subscribed to the topic's forum notifications. 
You can unsubscribe from your profile on our.umbraco.com";

                        var mailMessage = new MailMessage
                        {
                            Subject = subject,
                            Body = body
                        };

                        mailMessage.To.Add(member.GetPropertyValue<string>("Email"));
                        mailMessage.From = @from;

                        using (var smtpClient = new SmtpClient())
                        {
                            smtpClient.Send(mailMessage);
                        }
                    }
                    catch (Exception exception)
                    {
                        LogHelper.Error<NewForumComment>(
                            string.Format("Error sending mail to member id {0}", memberId), exception);
                    }
                }
            }
        }
    }
}
