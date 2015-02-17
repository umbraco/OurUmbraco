using NotificationsWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace NotificationsWeb.Services
{
    public class NotificationService: IDisposable
    {
        private DatabaseContext DatabaseContext;

        public NotificationService()
        {
            init(ApplicationContext.Current.DatabaseContext);
        }

        public NotificationService(DatabaseContext dbContext)
        {
            init(dbContext);
        }

        private void init(DatabaseContext dbContext)
        {
            DatabaseContext = dbContext;
        }

        public void SubscribeToForumTopic(int topicId, int memberId)
        {
            var r = DatabaseContext.Database.SingleOrDefault<ForumTopicSubscriber>(
                "SELECT * FROM forumtopicsubscribers WHERE topicId=@0 and memberId=@1", 
                topicId,
                memberId);

            if(r == null)
            {
                var rec = new ForumTopicSubscriber();
                rec.MemberId = memberId;
                rec.TopicId = topicId;

                DatabaseContext.Database.Insert(rec);
            }

        }

        public void UnSubscribeFromForumTopic(int topicId, int memberId)
        {
            DatabaseContext.Database.Delete<ForumTopicSubscriber>(
                "Where topicId=@0 and memberId=@1",
                topicId,
                memberId);
                
        }

        public void RemoveAllTopicSubscriptions(int topicId)
        {
            DatabaseContext.Database.Delete<ForumTopicSubscriber>(
               "Where topicId=@0",
               topicId);
        }

        public bool IsSubscribedToTopic(int topicId, int memberId)
        {
            return DatabaseContext.Database.SingleOrDefault<ForumTopicSubscriber>(
                 "SELECT * FROM forumtopicsubscribers WHERE topicId=@0 and memberId=@1",
                 topicId,
                 memberId) != null;

        }
        public void SubscribeToForum(int forumId, int memberId)
        {
            var r = DatabaseContext.Database.SingleOrDefault<ForumSubscriber>(
                "SELECT * FROM forumsubscribers WHERE forumId=@0 and memberId=@1",
                forumId,
                memberId);

            if (r == null)
            {
                var rec = new ForumSubscriber();
                rec.MemberId = memberId;
                rec.ForumId = forumId;

                DatabaseContext.Database.Insert(rec);
            }
        }

        public void UnSubscribeFromForum(int forumId, int memberId)
        {
            DatabaseContext.Database.Delete<ForumSubscriber>(
               "Where forumId=@0 and memberId=@1",
               forumId,
               memberId);
        }

        public void RemoveAllForumSubscriptions(int forumId)
        {
            DatabaseContext.Database.Delete<ForumSubscriber>(
                "Where forumId=@0",
                forumId);
        }

        public bool IsSubscribedToForum(int forumId, int memberId)
        {
            return DatabaseContext.Database.SingleOrDefault<ForumSubscriber>(
                 "SELECT * FROM forumsubscribers WHERE forumId=@0 and memberId=@1",
                 forumId,
                 memberId) != null;
        }
        public Page<ForumSubscriber> GetForumSubscriptionsFromMember(int memberId, long take = 50, long page = 1)
        {
            var sql = new Sql()
                .Select("*")
                .From<ForumSubscriber>();

            sql.Where<ForumSubscriber>(x => x.MemberId == memberId);

            return DatabaseContext.Database.Page<ForumSubscriber>(page, take, sql);

        }

        public Page<ForumTopicSubscriber> GetTopicSubscriptionsFromMember(int memberId, long take = 50, long page = 1)
        {
            var sql = new Sql()
                .Select("*")
                .From<ForumTopicSubscriber>();

            sql.Where<ForumTopicSubscriber>(x => x.MemberId == memberId);

            return DatabaseContext.Database.Page<ForumTopicSubscriber>(page, take, sql);

        }

        public static NotificationService Instance
        {
            get
            {
                return Singleton<NotificationService>.UniqueInstance;
            }
        }

        public void Dispose()
        {
            DatabaseContext.DisposeIfDisposable();
        }
    }
}
