using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using uForum.Models;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Persistence;

namespace uForum.Services
{
    public class TopicService
    {
        private readonly DatabaseContext _databaseContext;

        public TopicService(DatabaseContext dbContext)
        {
            _databaseContext = dbContext;
        }

        /// <summary>
        /// Returns a paged set of topics with the author information - without the comments loaded
        /// </summary>
        /// <param name="take"></param>
        /// <param name="page"></param>
        /// <param name="ignoreSpam"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public Page<ReadOnlyTopic> GetLatestTopics(long take = 50, long page = 1, bool ignoreSpam = true, int category = -1)
        {
            var sql = new Sql().Select(@"forumTopics.*, u1.[text] as LastReplyAuthorName, u2.[text] as AuthorName")
                .From("forumTopics")
                .LeftOuterJoin("umbracoNode u1").On("(forumTopics.latestReplyAuthor = u1.id AND u1.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')")
                .LeftOuterJoin("umbracoNode u2").On("(forumTopics.memberId = u2.id AND u2.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')");

            if (ignoreSpam)
            {
                sql.Where<Topic>(x => x.IsSpam != true);
            }

            if (category > 0)
                sql.Where<Topic>(x => x.ParentId == category);


            sql.OrderByDescending("updated");
            return _databaseContext.Database.Page<ReadOnlyTopic>(page, take, sql);
        }

        /// <summary>
        /// Returns an in-memory collection of topics that a given member has participated in
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="ignoreSpam"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public IEnumerable<ReadOnlyTopic> GetLatestTopicsForMember(int memberId, bool ignoreSpam = true, int maxCount = 100)
        {
            var sql = new Sql().Select("TOP " + maxCount + " " +
                                       @"forumTopics.*, u1.[text] as LastReplyAuthorName, u2.[text] as AuthorName,
    forumComments.body as commentBody, forumComments.created as commentCreated, forumComments.haschildren, 
	forumComments.id as commentId, forumComments.isSpam as commentIsSpam, forumComments.memberId as commentMemberId, forumComments.parentCommentId,
	forumComments.position, forumComments.score, forumComments.topicId")
                .From("forumTopics")
                .LeftOuterJoin("forumComments").On("forumTopics.id = forumComments.topicId")
                .LeftOuterJoin("umbracoNode u1").On("(forumTopics.latestReplyAuthor = u1.id AND u1.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')")
                .LeftOuterJoin("umbracoNode u2").On("(forumTopics.memberId = u2.id AND u2.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')")
                .Where<Topic>(topic => topic.LatestReplyAuthor == memberId || topic.MemberId == memberId);

            if (ignoreSpam)
            {
                sql.Where<Topic>(x => x.IsSpam != true);
                sql.Where("forumComments.id IS NULL OR (forumComments.isSpam <> 1)");
            }

            sql
                .OrderByDescending<Topic>(x => x.Updated)
                .OrderByDescending<Comment>(comment => comment.Created);

            var result = _databaseContext.Database.Fetch<ReadOnlyTopic, ReadOnlyComment, ReadOnlyTopic>(
                new TopicCommentRelator().Map,
                sql);

            return result;
        }

        /// <summary>
        /// Returns a READER of all topics to be iterated over
        /// </summary>
        /// <param name="ignoreSpam"></param>
        /// <param name="maxCount">
        /// Default is 1000
        /// </param>
        /// <returns></returns>
        public IEnumerable<ReadOnlyTopic> QueryAll(bool ignoreSpam = true, int maxCount = 1000)
        {
            var sql = new Sql().Select("TOP " + maxCount + " " + 
        @"forumTopics.*, u1.[text] as LastReplyAuthorName, u2.[text] as AuthorName,
    forumComments.body as commentBody, forumComments.created as commentCreated, forumComments.haschildren, 
	forumComments.id as commentId, forumComments.isSpam as commentIsSpam, forumComments.memberId as commentMemberId, forumComments.parentCommentId,
	forumComments.position, forumComments.score, forumComments.topicId")
                .From("forumTopics")
                .LeftOuterJoin("forumComments").On("forumTopics.id = forumComments.topicId")
                .LeftOuterJoin("umbracoNode u1").On("(forumTopics.latestReplyAuthor = u1.id AND u1.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')")
                .LeftOuterJoin("umbracoNode u2").On("(forumTopics.memberId = u2.id AND u2.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')");

            if (ignoreSpam)
            {
                sql.Where<Topic>(x => x.IsSpam != true);
                sql.Where("forumComments.id IS NULL OR (forumComments.isSpam <> 1)");
            }

            sql.Where("forumComments.id IS NULL OR (forumComments.[parentCommentId] = 0)");

            //start with the most recent
            sql
                .OrderByDescending<Topic>(x => x.Updated)
                .OrderByDescending<Comment>(comment => comment.Created);

            var result = _databaseContext.Database.Query<ReadOnlyTopic, ReadOnlyComment, ReadOnlyTopic>(
                new TopicCommentRelator().Map,
                sql);

            return result;
        }

        /// <summary>
        /// Returns a single topic including it's comments and author information
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ReadOnlyTopic QueryById(int id)
        {
            var sql = new Sql().Select(@"forumTopics.*,  u1.[text] as LastReplyAuthorName, u2.[text] as AuthorName,
    forumComments.body as commentBody, forumComments.created as commentCreated, forumComments.haschildren, 
	forumComments.id as commentId, forumComments.isSpam as commentIsSpam, forumComments.memberId as commentMemberId, forumComments.parentCommentId,
	forumComments.position, forumComments.score, forumComments.topicId")
                .From("forumTopics")
                .LeftOuterJoin("forumComments").On("forumTopics.id = forumComments.topicId")
                .LeftOuterJoin("umbracoNode u1").On("(forumTopics.latestReplyAuthor = u1.id AND u1.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')")
                .LeftOuterJoin("umbracoNode u2").On("(forumTopics.memberId = u2.id AND u2.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')")
                .Where<ReadOnlyTopic>(topic => topic.Id == id);

            return _databaseContext.Database.Query<ReadOnlyTopic, ReadOnlyComment, ReadOnlyTopic>(
                new TopicCommentRelator().Map,
                sql).FirstOrDefault();
        }

        public Topic GetById(int id)
        {
            return _databaseContext.Database.SingleOrDefault<Topic>(id);
        }

        /* CRUD */
        public Topic Save(Topic topic, bool raiseEvents = true)
        {
            var newTopic = topic.Id <= 0;
            var eventArgs = new TopicEventArgs() { Topic = topic };

            if (raiseEvents)
            {
                if (newTopic)
                    Creating.Raise(this, eventArgs);
                else
                    Updating.Raise(this, eventArgs);
            }

            if (!eventArgs.Cancel)
            {


                //save entity
                _databaseContext.Database.Save(topic);

                if (raiseEvents)
                {
                    if (newTopic)
                        Created.Raise(this, eventArgs);
                    else
                        Updated.Raise(this, eventArgs);
                }

            }
            else
            {
                CancelledByEvent.Raise(this, eventArgs);
            }

            return topic;
        }


        public void ChangeCategory(Topic topic, int newCategory)
        {
            var eventArgs = new TopicEventArgs() { Topic = topic };
            if (Moving.RaiseAndContinue(this, eventArgs))
            {
                topic.ParentId = newCategory;
                _databaseContext.Database.Save(topic);

                Moved.Raise(this, eventArgs);
            }
            else
                CancelledByEvent.Raise(this, eventArgs);
        }


        public void Lock(Topic topic)
        {
            var eventArgs = new TopicEventArgs() { Topic = topic };
            if (Locking.RaiseAndContinue(this, eventArgs))
            {
                topic.Locked = true;
                _databaseContext.Database.Save(topic);
                Locked.Raise(this, eventArgs);
            }
            else
                CancelledByEvent.Raise(this, eventArgs);
        }

        public void Delete(Topic topic)
        {
            var eventArgs = new TopicEventArgs() { Topic = topic };
            if (Deleting.RaiseAndContinue(this, eventArgs))
            {
                _databaseContext.Database.Delete(topic);
                Deleted.Raise(this, eventArgs);
            }
            else
                CancelledByEvent.Raise(this, eventArgs);
        }

        /// <summary>
        /// Get the current forum topic based on the id in the context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cache"></param>
        /// <returns></returns>
        /// <remarks>
        /// So that we don't have to look this up multiple times in a single request, this will use the given ICacheProvider to cache it
        /// </remarks>
        public ReadOnlyTopic CurrentTopic(HttpContextBase context, ICacheProvider cache)
        {
            return (ReadOnlyTopic)cache.GetCacheItem(typeof (TopicService) + "-CurrentTopic", () =>
            {
                var contextId = context.Items["topicID"];
                if (contextId != null)
                {
                    int topicId = 0;
                    if (int.TryParse(contextId.ToString(), out topicId))
                        return QueryById(topicId);
                }

                return null;
            });
        }

        public static event EventHandler<TopicEventArgs> Created;
        public static event EventHandler<TopicEventArgs> Creating;

        public static event EventHandler<TopicEventArgs> Deleted;
        public static event EventHandler<TopicEventArgs> Deleting;

        public static event EventHandler<TopicEventArgs> Moved;
        public static event EventHandler<TopicEventArgs> Moving;

        public static event EventHandler<TopicEventArgs> Locked;
        public static event EventHandler<TopicEventArgs> Locking;

        public static event EventHandler<TopicEventArgs> Updated;
        public static event EventHandler<TopicEventArgs> Updating;

        public static event EventHandler<TopicEventArgs> MarkedAsSpam;
        public static event EventHandler<TopicEventArgs> MarkingAsSpam;

        public static event EventHandler<TopicEventArgs> MarkedAsHam;
        public static event EventHandler<TopicEventArgs> MarkingAsHam;

        public static event EventHandler<TopicEventArgs> CancelledByEvent;

    }
}
