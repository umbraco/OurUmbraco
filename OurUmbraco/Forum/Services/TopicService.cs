using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OurUmbraco.Forum.Extensions;
using OurUmbraco.Forum.Models;
using OurUmbraco.NotificationsWeb.Library;
using OurUmbraco.Our.Models;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web;

namespace OurUmbraco.Forum.Services
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
        public IEnumerable<ReadOnlyTopic> GetLatestTopics(long take = 50, long page = 1, bool ignoreSpam = true, int category = -1)
        {
            const string sql1 = @"SELECT forumTopics.*, u1.[text] as LastReplyAuthorName, u2.[text] as AuthorName
FROM forumTopics
LEFT OUTER JOIN umbracoNode u1 ON (forumTopics.latestReplyAuthor = u1.id AND u1.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')
LEFT OUTER JOIN umbracoNode u2 ON (forumTopics.memberId = u2.id AND u2.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')
";
            const string sql2 = @"
ORDER BY updated DESC
OFFSET @offset ROWS
FETCH NEXT @count ROWS ONLY";

            const string sqlxx = sql1 + sql2;
            const string sqlix = sql1 + "WHERE isSpam=0" + sql2;
            const string sqlxc = sql1 + "WHERE forumTopics.parentId=@category" + sql2;
            const string sqlic = sql1 + "WHERE isSpam=0 AND forumTopics.parentId=@category" + sql2;

            var sql = ignoreSpam
                ? (category > 0 ? sqlic : sqlix)
                : (category > 0 ? sqlxc : sqlxx);

            // probably as fast as PetaPoco can be...
            return _databaseContext.Database.Fetch<ReadOnlyTopic>(sql, new
            {
                offset = (page - 1) * take,
                count = take,
                category = category
            });
        }

        public IEnumerable<ReadOnlyTopic> GetLatestTopicsFiltered(long take = 50, long page = 1, bool ignoreSpam = true,
            int category = -1, bool unsolved = false, bool noreplies = false)
        {
            const string sql1 = @"SELECT forumTopics.*, u1.[text] as LastReplyAuthorName, u2.[text] as AuthorName
FROM forumTopics
LEFT OUTER JOIN umbracoNode u1 ON (forumTopics.latestReplyAuthor = u1.id AND u1.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')
LEFT OUTER JOIN umbracoNode u2 ON (forumTopics.memberId = u2.id AND u2.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')
";
            const string sql2 = @"
ORDER BY updated DESC
OFFSET @offset ROWS
FETCH NEXT @count ROWS ONLY";

            const string sqlix = sql1 + "WHERE isSpam=0";
            const string sqlxc = sql1 + "WHERE forumTopics.parentId=@category";
            const string sqlic = sql1 + "WHERE isSpam=0 AND forumTopics.parentId=@category";

            var sql = ignoreSpam
                ? (category > 0 ? sqlic : sqlix)
                : (category > 0 ? sqlxc : sql1);

            if (unsolved)
                if (sql.Contains("WHERE"))
                    sql = sql + " AND answer = 0";
                else
                    sql = sql + " WHERE answer = 0";

            if (noreplies)
                if (sql.Contains("WHERE"))
                    sql = sql + " AND replies = 0";
                else
                    sql = sql + " WHERE replies = 0";

            sql = sql + sql2;

            // probably as fast as PetaPoco can be...
            return _databaseContext.Database.Fetch<ReadOnlyTopic>(sql, new
            {
                offset = (page - 1) * take,
                count = take,
                category = category
            });
        }

        /// <summary>
        /// Returns a set of topics for a specific author
        /// </summary>
        /// 
        /// <param name="memberId"></param>
        /// <returns></returns>
        public IEnumerable<ReadOnlyTopic> GetAuthorLatestTopics(int memberId)
        {
            const string sql = @"SELECT * FROM forumTopics WHERE memberId=@memberId";
            return _databaseContext.Database.Fetch<ReadOnlyTopic>(sql, new { memberId });
        }

        /// <summary>
        /// Returns a count of all topics
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int GetAllTopicsCount(int category = -1, bool unsolved = false, bool noreplies = false)
        {
            const string sql1 = @"SELECT COUNT(*) as forumTopicCount
FROM forumTopics
LEFT OUTER JOIN umbracoNode u1 ON (forumTopics.latestReplyAuthor = u1.id AND u1.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')
LEFT OUTER JOIN umbracoNode u2 ON (forumTopics.memberId = u2.id AND u2.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')
";
            const string sqlix = sql1 + "WHERE isSpam=0";
            const string sqlic = sql1 + "WHERE isSpam=0 AND forumTopics.parentId=@category";

            var sql = (category > 0 ? sqlic : sqlix);

            if (unsolved)
                if (sql.Contains("WHERE"))
                    sql = sql + " AND answer = 0";
                else
                    sql = sql + " WHERE answer = 0";

            if (noreplies)
                if (sql.Contains("WHERE"))
                    sql = sql + " AND replies = 0";
                else
                    sql = sql + " WHERE replies = 0";

            return _databaseContext.Database.ExecuteScalar<int>(sql, new { category = category });
        }

        /// <summary>
        /// Returns all topics in a certain date range
        /// </summary>
        public IEnumerable<Topic> GetAllTopics(bool unsolved = false, bool noreplies = false)
        {
            var sql = @"SELECT * FROM forumTopics WHERE isSpam=0";

            if (unsolved)
                if (sql.Contains("WHERE"))
                    sql = sql + " AND answer = 0";
                else
                    sql = sql + " WHERE answer = 0";

            if (noreplies)
                if (sql.Contains("WHERE"))
                    sql = sql + " AND replies = 0";
                else
                    sql = sql + " WHERE replies = 0";

            return _databaseContext.Database.Fetch<Topic>(sql);
        }

        /// <summary>
        /// Returns a count of all topics
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int GetAllTopicsCountByDateRange(DateTime fromDate, DateTime toDate, int category = -1, bool unsolved = false, bool noreplies = false)
        {
            const string sql1 = @"SELECT COUNT(*) as forumTopicCount
FROM forumTopics
LEFT OUTER JOIN umbracoNode u1 ON (forumTopics.latestReplyAuthor = u1.id AND u1.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')
LEFT OUTER JOIN umbracoNode u2 ON (forumTopics.memberId = u2.id AND u2.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')
";
            const string sqlix = sql1 + "WHERE isSpam=0";
            const string sqlic = sql1 + "WHERE isSpam=0 AND forumTopics.parentId=@category";

            var sql = (category > 0 ? sqlic : sqlix);

            if (unsolved)
                if (sql.Contains("WHERE"))
                    sql = sql + " AND answer = 0";
                else
                    sql = sql + " WHERE answer = 0";

            if (noreplies)
                if (sql.Contains("WHERE"))
                    sql = sql + " AND replies = 0";
                else
                    sql = sql + " WHERE replies = 0";

            if (sql.Contains("WHERE"))
                sql = sql + " AND (forumTopics.Created BETWEEN '" + fromDate.ToString("yyyy-MM-dd") + "' AND '" + toDate.ToString("yyyy-MM-dd") + "')";
            else
                sql = sql + " WHERE (forumTopics.Created BETWEEN '" + fromDate.ToString("yyyy-MM-dd") + "' AND '" + toDate.ToString("yyyy-MM-dd") + "')";

            return _databaseContext.Database.ExecuteScalar<int>(sql, new { category = category });
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
            var sql1 = @"SELECT TOP " + maxCount + @" forumTopics.*, u1.[text] as LastReplyAuthorName, u2.[text] as AuthorName, u2.[id] as topicAuthorId,
    forumComments.body as commentBody, forumComments.created as commentCreated, forumComments.haschildren, 
	forumComments.id as commentId, forumComments.isSpam as commentIsSpam, forumComments.memberId as commentMemberId, forumComments.parentCommentId,
	forumComments.position, forumComments.score, forumComments.topicId
FROM forumTopics
LEFT OUTER JOIN forumComments ON (forumTopics.id = forumComments.topicId)
LEFT OUTER JOIN umbracoNode u1 ON (forumTopics.latestReplyAuthor = u1.id AND u1.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')
LEFT OUTER JOIN umbracoNode u2 ON (forumTopics.memberId = u2.id AND u2.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')
WHERE
";
            const string sql2 = @"
    (forumComments.id IS NULL OR forumComments.parentCommentId=0)
ORDER BY forumTopics.updated DESC, forumComments.created DESC
";

            var sqlx = sql1 + sql2;
            var sqli = sql1 + " (forumTopics.isSpam <> 1) AND (forumComments.id IS NULL OR forumComments.isSpam <> 1) AND " + sql2;

            var sql = ignoreSpam ? sqlx : sqli;

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
            const string sql = @"SELECT forumTopics.*,  u1.[text] as LastReplyAuthorName, u2.[text] as AuthorName, u2.[id] as topicAuthorId,
    forumComments.body as commentBody, forumComments.created as commentCreated, forumComments.haschildren, 
	forumComments.id as commentId, forumComments.isSpam as commentIsSpam, forumComments.memberId as commentMemberId, forumComments.parentCommentId,
	forumComments.position, forumComments.score, forumComments.topicId, u3.text AS solvedMemberName
FROM forumTopics
LEFT OUTER JOIN forumComments ON (forumTopics.id = forumComments.topicId)
LEFT OUTER JOIN umbracoNode u1 ON (forumTopics.latestReplyAuthor = u1.id AND u1.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')
LEFT OUTER JOIN umbracoNode u2 ON (forumTopics.memberId = u2.id AND u2.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')
LEFT OUTER JOIN umbracoNode u3 ON (forumTopics.answer = u3.id AND u3.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560')
WHERE forumTopics.id=@id
";

            var results = _databaseContext.Database.Fetch<ReadOnlyTopic, ReadOnlyComment, ReadOnlyTopic>(
                new TopicCommentRelator().Map,
                sql, new { id = id }).FirstOrDefault();

            if (results != null)
            {
                const string topicVoteQuery = @"SELECT memberId, umbracoNode.text AS memberName FROM powersTopic LEFT JOIN umbracoNode ON (powersTopic.memberId = umbracoNode.Id) WHERE powersTopic.id = @id AND receiverId != 0";
                var topicVotes = _databaseContext.Database.Fetch<SimpleMember>(topicVoteQuery, new { id = id });
                results.Votes = topicVotes;

                if (results.Comments.Any())
                {
                    const string commentsVotesQuery = @"SELECT powersComment.id, powersComment.memberId, umbracoNode.text AS memberName FROM powersComment LEFT JOIN umbracoNode ON (powersComment.memberId = umbracoNode.id) WHERE powersComment.id in (SELECT id FROM forumComments WHERE topicId = @id) AND receiverId != 0";
                    var commentsVotes = _databaseContext.Database.Fetch<SimpleMember>(commentsVotesQuery, new { id = id });

                    foreach (var readOnlyComment in results.Comments)
                    {
                        var votes = commentsVotes.Where(x => x.CommentId == readOnlyComment.Id);
                        readOnlyComment.Votes = new List<SimpleMember>();
                        foreach (var simpleMember in votes)
                        {
                            readOnlyComment.Votes.Add(simpleMember);
                        }
                    }
                }
            }

            return results;
        }

        public Topic GetById(int id)
        {
            return _databaseContext.Database.SingleOrDefault<Topic>(id);
        }

        /* CRUD */
        public Topic Save(Topic topic)
        {
            var newTopic = topic.Id <= 0;
            var eventArgs = new TopicEventArgs() { Topic = topic };

            if (newTopic)
                Creating.Raise(this, eventArgs);
            else
                Updating.Raise(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                //save entity
                _databaseContext.Database.Save(topic);

                if (newTopic)
                    Created.Raise(this, eventArgs);
                else
                    Updated.Raise(this, eventArgs);

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
        /// <param name="memberData"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <remarks>
        /// So that we don't have to look this up multiple times in a single request, this will use the given ICacheProvider to cache it
        /// </remarks>
        public ReadOnlyTopic CurrentTopic(HttpContextBase context, ICacheProvider cache, MemberData memberData, IPublishedContent content)
        {
            var topic = (ReadOnlyTopic)cache.GetCacheItem(typeof(TopicService) + "-CurrentTopic", () =>
            {
                var contextId = context.Items["topicID"] as string;
                if (contextId == null)
                    return null;

                return int.TryParse(contextId, out var topicId) ? QueryById(topicId) : null;
            });

            if (topic == null)
                return null;

            topic.MemberData = memberData;
            if (content != null)
            {
                topic.ForumNewTopicsAllowed = content.NewTopicsAllowed();
                topic.MainNotification = content.GetPropertyValue<string>("mainNotification");
                if (string.IsNullOrWhiteSpace(topic.MainNotification))
                    topic.MainNotification = "This forum is in read only mode, you can no longer reply";
                topic.ForumName = content.Name;
                topic.ForumUrl = content.Url;
            }

            if (memberData != null && memberData.Member != null)
                topic.Subscribed = Utils.IsSubscribedToForumTopic(topic.Id, memberData.Member.Id);

            var currentMember = memberData != null && memberData.Member != null
                ? memberData.Member
                : null;

            var topicAuthorIsCurrentMember = currentMember != null && topic.MemberId == currentMember.Id;

            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            topic.TopicAuthor = topicAuthorIsCurrentMember
                ? currentMember
                : umbracoHelper.TypedMember(topic.MemberId);

            topic.TopicMembers = new List<TopicMember>();
            if (topic.TopicAuthor != null)
            {
                var author = new TopicMember
                {
                    Member = topic.TopicAuthor,
                    Roles = topicAuthorIsCurrentMember ? memberData.Roles : topic.TopicAuthor.GetRoles()
                };
                topic.TopicMembers.Add(author);
            }

            foreach (var comment in topic.Comments)
            {
                if (topic.TopicMembers.Any(x => x.Member.Id == comment.MemberId))
                    continue;

                var commentAuthorIsCurrentMember = currentMember != null && comment.MemberId == currentMember.Id;
                if (commentAuthorIsCurrentMember)
                {
                    var commentAuthor = new TopicMember { Member = currentMember, Roles = memberData.Roles };
                    topic.TopicMembers.Add(commentAuthor);
                }
                else
                {
                    var commentMember = umbracoHelper.TypedMember(comment.MemberId);
                    if (commentMember == null)
                        continue;

                    var commenterRoles = commentMember.GetRoles();
                    var commentAuthor = new TopicMember { Member = commentMember, Roles = commenterRoles };
                    topic.TopicMembers.Add(commentAuthor);
                }
            }

            foreach (var comment in topic.Comments)
            {
                comment.TopicMembers = topic.TopicMembers;
                comment.MemberData = memberData;
                if (topic.Answer == comment.Id)
                    comment.IsAnswer = true;
                var forumContent = umbracoHelper.TypedContent(topic.ParentId);
                comment.ForumNewTopicsAllowed = forumContent.NewTopicsAllowed();
            }

            return topic;
        }


        public void SendNotifications(ReadOnlyTopic topic, string memberName, string url)
        {
            var newForumTopicNotification = new NotificationsCore.Notifications.NewForumTopic();
            newForumTopicNotification.SendNotification(topic, memberName, url);
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
