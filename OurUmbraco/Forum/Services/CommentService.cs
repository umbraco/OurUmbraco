using System;
using System.Collections.Generic;
using OurUmbraco.Forum.Extensions;
using OurUmbraco.Forum.Models;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace OurUmbraco.Forum.Services
{
    /// <summary>
    /// Used for CRUD of comments - There aren't any query methods for comments here, comments are resolved with a Topic in a single query be displayed in the view
    /// </summary>
    public class CommentService
    {

        private readonly DatabaseContext _databaseContext;
        private readonly TopicService _topicService;

        public CommentService(DatabaseContext dbContext, TopicService topicService)
        {
            if (dbContext == null) throw new ArgumentNullException("dbContext");
            if (topicService == null) throw new ArgumentNullException("topicService");
            _databaseContext = dbContext;
            _topicService = topicService;
        }

        public Comment GetById(int id)
        {
            return _databaseContext.Database.SingleOrDefault<Comment>(id);
        }
        
        /// <summary>
        /// Returns all comments that a given member has created
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public IEnumerable<Comment> GetAllCommentsForMember(int memberId)
        {
            var sql = new Sql().Select("*").From("forumComments").Where<Comment>(comment => comment.MemberId == memberId);
            var result = _databaseContext.Database.Fetch<Comment>(sql);
            return result;
        }


        /* Crud */
        public Comment Save(Comment comment, bool updateTopicPostCount = true)
        {
            var newComment = comment.Id <= 0;
            var eventArgs = new CommentEventArgs() { Comment = comment };

            if (newComment)
                Creating.Raise(this, eventArgs);
            else
                Updating.Raise(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                //save comment
                _databaseContext.Database.Save(comment);


                //topic post count
                if(updateTopicPostCount)
                    UpdateTopicPostsCount(comment);

                //parent comment state
                if (comment.ParentCommentId > 0)
                {
                    var p = GetById(comment.ParentCommentId);
                    if (p != null)
                        p.HasChildren = true;
                    Save(p, false);
                }

                if (newComment)
                    Created.Raise(this, eventArgs);
                else
                    Updated.Raise(this, eventArgs);
            }
            else
            {
                CancelledByEvent.Raise(this, eventArgs);
            }

            return comment;
        }

        private void UpdateTopicPostsCount(Comment c, bool adding = true)
        {
            var ts = _topicService;
            var t = ts.GetById(c.TopicId);
            t.Replies = adding ? t.Replies + 1 : t.Replies - 1;
            t.Updated = DateTime.Now;

            if (adding)
                t.LatestReplyAuthor = c.MemberId;

            ts.Save(t);
        }

        public void Delete(Comment comment)
        {
            var eventArgs = new CommentEventArgs() { Comment = comment };
            if (Deleting.RaiseAndContinue(this, eventArgs))
            {
                UpdateTopicPostsCount(comment, false);
                _databaseContext.Database.Delete(comment);
                Deleted.Raise(this, eventArgs);
            }
            else
                CancelledByEvent.Raise(this, eventArgs);
        }


        public static event EventHandler<CommentEventArgs> Created;
        public static event EventHandler<CommentEventArgs> Creating;

        public static event EventHandler<CommentEventArgs> Deleted;
        public static event EventHandler<CommentEventArgs> Deleting;

        public static event EventHandler<CommentEventArgs> Updated;
        public static event EventHandler<CommentEventArgs> Updating;

        public static event EventHandler<CommentEventArgs> MarkedAsSpam;
        public static event EventHandler<CommentEventArgs> MarkingAsSpam;

        public static event EventHandler<CommentEventArgs> MarkedAsHam;
        public static event EventHandler<CommentEventArgs> MarkingAsHam;

        public static event EventHandler<CommentEventArgs> CancelledByEvent;

    }
}
