using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uForum.Models;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace uForum.Services
{
    public class CommentService : IDisposable
    {

        private DatabaseContext DatabaseContext;

        public CommentService()
        {
            init(ApplicationContext.Current.DatabaseContext);
        }

        public CommentService(DatabaseContext dbContext)
        {
            init(dbContext);
        }
        private void init(DatabaseContext dbContext)
        {
            DatabaseContext = dbContext;
        }

        
        public Page<Comment> GetPagedComments(int topicId, long number = 10, long page = 1, bool ignoreSpam = true)
        {
            var sql = new Sql()
                 .Select("*")
                 .From<Comment>();

           // if (ignoreSpam)
           //     sql.Where<Comment>(x => x.IsSpam != true);

            if (topicId > 0)
                sql.Where<Comment>(x => x.TopicId == topicId);

            sql.Where<Comment>(x => x.ParentCommentId == 0);
            sql.OrderByDescending("created");
            
            return DatabaseContext.Database.Page<Comment>(page, number, sql);
        }

        public IEnumerable<Comment> GetComments(int topicId, bool ignoreSpam = true)
        {
            var sql = new Sql()
                 .Select("*")
                 .From<Comment>();

            if (ignoreSpam)
                 sql.Where<Comment>(x => x.IsSpam != true);

            if (topicId > 0)
                sql.Where<Comment>(x => x.TopicId == topicId);

            sql.Where<Comment>(x => x.ParentCommentId == 0);
            sql.OrderByDescending("created");

            return DatabaseContext.Database.Fetch<Comment>(sql);
        }

        public IEnumerable<Comment> GetChildComments(int commentId)
        {
            var sql = new Sql()
                  .Select("*")
                  .From<Comment>();

            sql.Where<Comment>(x => x.ParentCommentId == commentId);
            sql.OrderByDescending("created");

            return DatabaseContext.Database.Query<Comment>(sql);
        }

        public Comment GetById(int id)
        {
            return DatabaseContext.Database.SingleOrDefault<Comment>(id);
        }

        /* Crud */
        public Comment Save(Comment comment, bool raiseEvents = true)
        {
            var newComment = comment.Id < 0;
            var eventArgs = new CommentEventArgs() { Comment =comment };

            if (raiseEvents)
            {
                if (newComment)
                    Creating.Raise(this, eventArgs);
                else
                    Updating.Raise(this, eventArgs);
            }

            if (!eventArgs.Cancel)
            {

                //spam filtering
                comment.DetectSpam();

                //save comment
                DatabaseContext.Database.Save(comment);

                //topic post count
                UpdateTopicPostsCount(comment);

                //parent comment state
                if (comment.ParentCommentId > 0)
                {
                    var p = GetById(comment.ParentCommentId);
                    if (p != null)
                        p.HasChildren = true;
                    Save(p);
                }

                if (raiseEvents)
                {
                    if (newComment)
                        Created.Raise(this, eventArgs);
                    else
                        Updated.Raise(this, eventArgs);
                }

            }
            else
            {
                CancelledByEvent.Raise(this, eventArgs);
            }

            return comment;
        }

        private void UpdateTopicPostsCount(Comment c, bool adding = true)
        {
            using (var ts = new TopicService())
            {
                var t = ts.GetById(c.TopicId);
                t.Replies = adding ? t.Replies + 1 : t.Replies - 1;
                t.Updated = DateTime.Now;

                if (adding)
                    t.LatestReplyAuthor = c.MemberId;
                
                ts.Save(t);
            }
        }

        public void Delete(Comment comment)
        {
            var eventArgs = new CommentEventArgs() { Comment = comment };
            if (Deleting.RaiseAndContinue(this, eventArgs))
            {
                UpdateTopicPostsCount(comment, false);
                DatabaseContext.Database.Delete(comment);
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


        public static CommentService Instance
        {
            get
            {
                return Singleton<CommentService>.UniqueInstance;
            }
        }

        public void Dispose()
        {
          
        }
    }
}
