using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using uForum.Models;
using uForum.Services;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace uForum
{
    public class NewForumHandler : ApplicationEventHandler
    {

        /// <summary>
        ///  This handler creates a forum entry in the forumForums table
        ///  When a forum node is created in the content tree, all forums are connected to a node
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Published += ContentService_Published;
            ContentService.Deleted += ContentService_Deleted;
        }

        void ContentService_Deleted(IContentService sender, Umbraco.Core.Events.DeleteEventArgs<Umbraco.Core.Models.IContent> e)
        {
            var fs = new ForumService(ApplicationContext.Current.DatabaseContext);
            foreach (var ent in e.DeletedEntities.Where(x => x.ContentType.Alias == "Forum"))
            {

                var f = fs.GetById(ent.Id);
                if (f != null)
                    fs.Delete(f);
            }
        }

        void ContentService_Published(Umbraco.Core.Publishing.IPublishingStrategy sender, Umbraco.Core.Events.PublishEventArgs<Umbraco.Core.Models.IContent> e)
        {
            var fs = new ForumService(ApplicationContext.Current.DatabaseContext);
            foreach (var ent in e.PublishedEntities.Where(x => x.ContentType.Alias == "Forum"))
            {
                Forum f = fs.GetById(ent.Id);

                if (f == null)
                {
                    f = new Forum();
                    f.Id = ent.Id;
                    f.ParentId = ent.ParentId;
                    f.SortOrder = ent.SortOrder;
                    f.TotalTopics = 0;
                    f.TotalComments = 0;
                    f.LatestPostDate = DateTime.Now;
                    f.LatestAuthor = 0;
                    f.LatestComment = 0;
                    f.LatestTopic = 0;
                    fs.Save(f);
                }
            }
        }
    }
}
