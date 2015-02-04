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
    public class ForumService : IDisposable
    {
        private DatabaseContext DatabaseContext;
        public ForumService()
        {
            init(ApplicationContext.Current.DatabaseContext);
        }
        public ForumService(DatabaseContext dbContext)
        {
            init(dbContext);
        }
        private void init(DatabaseContext dbContext)
        {
            DatabaseContext = dbContext;
        }

        public IEnumerable<Forum> GetForums(int rootId)
        {
            var sql = new Sql();
            sql.Where<Forum>(x => x.ParentId == rootId);
            sql.OrderBy<Forum>(x => x.SortOrder);
            return DatabaseContext.Database.Fetch<Forum>(sql);
        }

        public Forum GetById(int nodeId)
        {
            return DatabaseContext.Database.SingleOrDefault<Forum>(nodeId);
        }

        

        public void Delete(Forum forum)
        {
            var eventArgs = new ForumEventArgs() { Forum = forum };
            if (Deleting.RaiseAndContinue(this, eventArgs))
            {
                DatabaseContext.Database.Delete(forum);
                Deleted.Raise(this, eventArgs);
            }
            else
                CancelledByEvent.Raise(this, eventArgs);
        }

        public Forum Save(Forum forum, bool raiseEvents = true)
        {
            var newForum = forum.Id <= 0;
            var eventArgs = new ForumEventArgs() { Forum = forum };

            if (raiseEvents)
            {
                if (newForum)
                    Creating.Raise(this, eventArgs);
                else
                    Updating.Raise(this, eventArgs);
            }

            if (!eventArgs.Cancel)
            {
                DatabaseContext.Database.Save(forum);

                if (raiseEvents)
                {
                    if (newForum)
                        Created.Raise(this, eventArgs);
                    else
                        Updated.Raise(this, eventArgs);
                }

            }
            else
            {
                CancelledByEvent.Raise(this, eventArgs);
            }

            return forum;
        }

        public static ForumService Instance
        {
            get
            {
                return Singleton<ForumService>.UniqueInstance;
            }
        }

        public static event EventHandler<ForumEventArgs> Created;
        public static event EventHandler<ForumEventArgs> Creating;

        public static event EventHandler<ForumEventArgs> Deleted;
        public static event EventHandler<ForumEventArgs> Deleting;

        public static event EventHandler<ForumEventArgs> Updated;
        public static event EventHandler<ForumEventArgs> Updating;

        public static event EventHandler<ForumEventArgs> CancelledByEvent;

        public void Dispose()
        {
            
        }
    }
}
