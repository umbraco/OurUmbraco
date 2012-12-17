using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace uForum {
    public class NewForumHandler : umbraco.BusinessLogic.ApplicationBase {
        public NewForumHandler() {
            umbraco.cms.businesslogic.web.Document.AfterPublish += new umbraco.cms.businesslogic.web.Document.PublishEventHandler(Document_AfterPublish);
            umbraco.cms.businesslogic.web.Document.AfterDelete += new umbraco.cms.businesslogic.web.Document.DeleteEventHandler(Document_AfterDelete);
        }

        void Document_AfterDelete(umbraco.cms.businesslogic.web.Document sender, umbraco.cms.businesslogic.DeleteEventArgs e) {
            if (sender.ContentType.Alias == "Forum") {

                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, 0, "forum has been deleted");
                
                Businesslogic.Forum f = new uForum.Businesslogic.Forum(sender.Id);

                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, f.Id, f.Title);

                f.Delete();
               
            }
        }
        
        void Document_AfterPublish(umbraco.cms.businesslogic.web.Document sender, umbraco.cms.businesslogic.PublishEventArgs e) {

            if (sender.ContentType.Alias == "Forum") {
                
                Businesslogic.Forum f = new uForum.Businesslogic.Forum(sender.Id);
                
                if (!f.Exists) {
                    f.Id = sender.Id;
                    f.ParentId = sender.Parent.Id;
                    f.SortOrder = sender.sortOrder;
                }

                f.Save();
            }
            
        }
    }
}
