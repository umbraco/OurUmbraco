using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using uForum.Models;
using uForum.Services;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace uForum {
    public class NewForumHandler : ApplicationEventHandler {

        /*
         * This handler creates a forum entry in the forumForums table
         * When a forum node is created in the content tree, all forums are connected to a node
         */
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Published += ContentService_Published;
            ContentService.Deleted += ContentService_Deleted;
        }

        void ContentService_Deleted(IContentService sender, Umbraco.Core.Events.DeleteEventArgs<Umbraco.Core.Models.IContent> e)
        {
           foreach(var ent in e.DeletedEntities.Where(x => x.ContentType.Alias == "Forum")){
                using (var fs = new ForumService())
                {
                    var f = fs.GetById(ent.Id);
                    if(f != null)
                        fs.Delete(f);
                }
            }
        }

        void ContentService_Published(Umbraco.Core.Publishing.IPublishingStrategy sender, Umbraco.Core.Events.PublishEventArgs<Umbraco.Core.Models.IContent> e)
        {
            foreach(var ent in e.PublishedEntities.Where(x => x.ContentType.Alias == "Forum")){
                
                using (var fs = new ForumService())
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
                        fs.Save(f);
                    }
                
                }
            }
        }
    }
}
