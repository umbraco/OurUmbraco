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

        
        public IEnumerable<Comment> GetComments(int topicId, long number = 10, long page = 1)
        {
            var sql = new Sql();
            
            if(topicId > 0)
                sql.Where<Comment>(x => x.TopicId == topicId);

            sql.OrderBy<Comment>(x => x.Created);
            return DatabaseContext.Database.Page<Comment>(page, number, sql).Items;
        }

        public IEnumerable<Comment> GetChildComments(int commentId)
        {
            var sql = new Sql();

            sql.Where<Comment>(x => x.ParentCommentId == commentId);

            sql.OrderBy<Comment>(x => x.Created);
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
                DatabaseContext.Database.Save(comment);

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

        public void Delete(Comment comment)
        {
            var eventArgs = new CommentEventArgs() { Comment = comment };
            if (Deleting.RaiseAndContinue(this, eventArgs))
            {
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


        public static CommentService Instance()
        {
            return Singleton<CommentService>.UniqueInstance;
        }

        public void Dispose()
        {
          
        }
    }
}
