﻿using NotificationsWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace NotificationsWeb.Services
{
    public class NotificationService
    {
        private readonly DatabaseContext _databaseContext;

        public NotificationService(DatabaseContext dbContext)
        {
            if (dbContext == null) throw new ArgumentNullException("dbContext");
            _databaseContext = dbContext;
        }


        public void SubscribeToForumTopic(int topicId, int memberId)
        {
            var r = _databaseContext.Database.SingleOrDefault<ForumTopicSubscriber>(
                "SELECT * FROM forumtopicsubscribers WHERE topicId=@0 and memberId=@1", 
                topicId,
                memberId);

            if(r == null)
            {
                var rec = new ForumTopicSubscriber();
                rec.MemberId = memberId;
                rec.TopicId = topicId;

                _databaseContext.Database.Insert(rec);
            }

        }

        public void UnSubscribeFromForumTopic(int topicId, int memberId)
        {
            _databaseContext.Database.Delete<ForumTopicSubscriber>(
                "Where topicId=@0 and memberId=@1",
                topicId,
                memberId);
                
        }

        public void RemoveAllTopicSubscriptions(int topicId)
        {
            _databaseContext.Database.Delete<ForumTopicSubscriber>(
               "Where topicId=@0",
               topicId);
        }

        public bool IsSubscribedToTopic(int topicId, int memberId)
        {
            return _databaseContext.Database.SingleOrDefault<ForumTopicSubscriber>(
                 "SELECT * FROM forumtopicsubscribers WHERE topicId=@0 and memberId=@1",
                 topicId,
                 memberId) != null;

        }
        public void SubscribeToForum(int forumId, int memberId)
        {
            var r = _databaseContext.Database.SingleOrDefault<ForumSubscriber>(
                "SELECT * FROM forumsubscribers WHERE forumId=@0 and memberId=@1",
                forumId,
                memberId);

            if (r == null)
            {
                var rec = new ForumSubscriber();
                rec.MemberId = memberId;
                rec.ForumId = forumId;

                _databaseContext.Database.Insert(rec);
            }
        }

        public void UnSubscribeFromForum(int forumId, int memberId)
        {
            _databaseContext.Database.Delete<ForumSubscriber>(
               "Where forumId=@0 and memberId=@1",
               forumId,
               memberId);
        }

        public void RemoveAllForumSubscriptions(int forumId)
        {
            _databaseContext.Database.Delete<ForumSubscriber>(
                "Where forumId=@0",
                forumId);
        }

        public bool IsSubscribedToForum(int forumId, int memberId)
        {
            return _databaseContext.Database.SingleOrDefault<ForumSubscriber>(
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

            return _databaseContext.Database.Page<ForumSubscriber>(page, take, sql);

        }

        public long GetNumberOfForumSubscriptionsFromMember(int memberId)
        {
            return _databaseContext.Database.ExecuteScalar<long>("SELECT Count(*) FROM forumSubscribers where memberId=@0",memberId);
        }

        public Page<ForumTopicSubscriber> GetTopicSubscriptionsFromMember(int memberId, long take = 50, long page = 1)
        {
            var sql = new Sql()
                .Select("*")
                .From<ForumTopicSubscriber>();

            sql.Where<ForumTopicSubscriber>(x => x.MemberId == memberId);

            return _databaseContext.Database.Page<ForumTopicSubscriber>(page, take, sql);

        }

        public long GetNumberOfTopicSubscriptionsFromMember(int memberId)
        {
            return _databaseContext.Database.ExecuteScalar<long>("SELECT Count(*) FROM forumTopicSubscribers where memberId=@0",memberId);
        }

    }
}
