using System;
using System.Collections.Generic;
using System.Text;
using NotificationsCore;
using umbraco.presentation.nodeFactory;
using umbraco.cms.businesslogic.member;
using Umbraco.Core;
using uForum.Services;
using uForum;
using NotificationsWeb.Services;

namespace NotificationsWeb.EventHandlers
{
    public class Forum : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //sub comment author to topic
            CommentService.Created += CommentService_Created;

            //sub owner to topic
            TopicService.Created += TopicService_Created;

            //If its a project forum, subscribe the owner to all topics
            ForumService.Created += ForumService_Created;

            //remove all forum subs
            ForumService.Deleted += ForumService_Deleted;
        }

        void ForumService_Deleted(object sender, uForum.ForumEventArgs e)
        {
            using (var ns = new NotificationService())
            {
                ns.RemoveAllForumSubscriptions(e.Forum.Id);
            }
            
        }

        void ForumService_Created(object sender, uForum.ForumEventArgs e)
        {
            var content = Umbraco.Web.UmbracoContext.Current.Application.Services.ContentService.GetById(e.Forum.ParentId);
            if (content.ContentType.Alias == "Project")
            {
                var owner = content.GetValue<int>("owner");
                //NotificationsWeb.BusinessLogic.Forum.Subscribe(e.Forum.Id, owner);
                using(var ns = new NotificationService())
                {
                    ns.SubscribeToForum(e.Forum.Id, owner);
                }
            }
        }

        void TopicService_Created(object sender, uForum.TopicEventArgs e)
        {
           
            using(var ns = new NotificationService())
            {
                ns.SubscribeToForumTopic(e.Topic.Id, e.Topic.MemberId);
            }

            //send notification
            InstantNotification not = new InstantNotification();

            //data for notification:
            var member = e.Topic.AuthorAsMember();
          

            not.Invoke(Config.ConfigurationFile, Config.AssemblyDir, "NewTopic", e.Topic,e.Topic.Url, member);
        }

        void CommentService_Created(object sender, uForum.CommentEventArgs e)
        {
            //Subscribe to topic
            using(var ns = new NotificationService())
            {
                ns.SubscribeToForumTopic(e.Comment.TopicId, e.Comment.MemberId);
            }
            //data for notification:
            var member = e.Comment.AuthorAsMember();
            var topic = TopicService.Instance.GetById(e.Comment.TopicId);

            //send notifications
            InstantNotification not = new InstantNotification();
            not.Invoke(Config.ConfigurationFile, Config.AssemblyDir, "NewComment", e.Comment, topic, topic.Url, member);
        }


    }
}
