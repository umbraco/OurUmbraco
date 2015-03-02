using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using uForum.Models;
using Umbraco.Core;
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

        /* Query */
        public Page<Topic> GetLatestTopics(long take = 50, long page = 1, bool ignoreSpam = true, int category = -1)
        {
            var sql = new Sql()
                .Select("*")
                .From<Topic>();

            //    if (ignoreSpam)
            //        sql.Where<Topic>(x => x.IsSpam != true);

            if (category > 0)
                sql.Where<Topic>(x => x.ParentId == category);


            sql.OrderByDescending("updated");
            return _databaseContext.Database.Page<Topic>(page, take, sql);
        }

        /// <summary>
        /// Returns a dataset in-memory of all topics
        /// </summary>
        /// <param name="ignoreSpam"></param>
        /// <returns></returns>
        public IEnumerable<Topic> GetAll(bool ignoreSpam = true)
        {
            var sql = new Sql();
            if (ignoreSpam)
                sql.Where<Topic>(x => x.IsSpam != true);

            sql.OrderBy<Topic>(x => x.Updated);

            return _databaseContext.Database.Fetch<Topic>(sql);
        }

        /// <summary>
        /// Returns a reader of all topics to be iterated over
        /// </summary>
        /// <param name="ignoreSpam"></param>
        /// <returns></returns>
        public IEnumerable<Topic> QueryAll(bool ignoreSpam = true)
        {
            var sql = new Sql();
            if (ignoreSpam)
                sql.Where<Topic>(x => x.IsSpam != true);

            sql.OrderBy<Topic>(x => x.Updated);

            return _databaseContext.Database.Query<Topic>(sql);
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

        /* Context */
        public Topic CurrentTopic(HttpContextBase context)
        {
            var contextId = context.Items["topicID"];
            if (contextId != null)
            {
                int topicId = 0;
                if (int.TryParse(contextId.ToString(), out topicId))
                    return GetById(topicId);
            }

            return null;
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
