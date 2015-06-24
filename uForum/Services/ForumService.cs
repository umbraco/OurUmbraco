using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uForum.Extensions;
using uForum.Models;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace uForum.Services
{
    public class ForumService
    {
        private readonly DatabaseContext _databaseContext;

        public ForumService(DatabaseContext dbContext)
        {
            if (dbContext == null) throw new ArgumentNullException("dbContext");
            _databaseContext = dbContext;
        }

        public IEnumerable<Forum> GetForums(int rootId)
        {
            var sql = new Sql();
            sql.Where<Forum>(x => x.ParentId == rootId);
            sql.OrderBy<Forum>(x => x.SortOrder);
            return _databaseContext.Database.Fetch<Forum>(sql);
        }

        public Forum GetById(int nodeId)
        {
            return _databaseContext.Database.SingleOrDefault<Forum>(nodeId);
        }

        public void Delete(Forum forum)
        {
            var eventArgs = new ForumEventArgs() { Forum = forum };
            if (Deleting.RaiseAndContinue(this, eventArgs))
            {
                _databaseContext.Database.Delete(forum);
                Deleted.Raise(this, eventArgs);
            }
            else
                CancelledByEvent.Raise(this, eventArgs);
        }

        public Forum Save(Forum forum)
        {
            var newForum = _databaseContext.Database.ExecuteScalar<int>("SELECT COUNT(*) FROM forumForums WHERE id=@id", new {id = forum.Id}) == 0;
            var eventArgs = new ForumEventArgs() { Forum = forum };

            if (newForum)
                Creating.Raise(this, eventArgs);
            else
                Updating.Raise(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                if (newForum)
                {
                    _databaseContext.Database.Insert(forum);
                    Created.Raise(this, eventArgs);
                }
                else
                {
                    _databaseContext.Database.Update(forum);
                    Updated.Raise(this, eventArgs);
                }
                    
            }
            else
            {
                CancelledByEvent.Raise(this, eventArgs);
            }

            return forum;
        }

    
        public static event EventHandler<ForumEventArgs> Created;
        public static event EventHandler<ForumEventArgs> Creating;

        public static event EventHandler<ForumEventArgs> Deleted;
        public static event EventHandler<ForumEventArgs> Deleting;

        public static event EventHandler<ForumEventArgs> Updated;
        public static event EventHandler<ForumEventArgs> Updating;

        public static event EventHandler<ForumEventArgs> CancelledByEvent;

       
    }
}
