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

        public void SubscribeToForumTopic(int topicId, int memberid)
        {

        }

        public void UnSubscribeFromForumTopic(int topicId, int memberId)
        {

        }

        public void RemoveAllTopicSubscriptions(int topicId)
        {

        }

        public bool IsSubscribedToTopic(int memberId)
        {
            throw new NotImplementedException();

        }
        public void SubscribeToForum(int forumId, int memberid)
        {

        }

        public void UnSubscribeFromForum(int forumId, int memberId)
        {

        }

        public void RemoveAllForumSubscriptions(int forumId)
        {

        }

        public bool IsSubscribedToForum(int memberId)
        {
            throw new NotImplementedException();
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
