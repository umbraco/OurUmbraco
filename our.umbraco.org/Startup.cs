using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Examine;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.Security;

namespace our
{
    public class StartupHandler : ApplicationEventHandler
    {
        private void CreateRoutes()
        {
            RouteTable.Routes.MapUmbracoRoute("Search", "search2/{term}",
                new
                {
                    Controller = "Search",
                    Action = "Search",
                    Term = UrlParameter.Optional
                },
                //NOTE: This virtual page will be routed as if it were the root 'community' page
                new UmbracoVirtualNodeByIdRouteHandler(1052));
        }

        private void BindExamineEvents()
        {
            ExamineManager.Instance.IndexProviderCollection["ProjectIndexer"].GatheringNodeData += ProjectIndexer_GatheringNodeData;
        }

        /// <summary>
        /// Need to ensures some custom data is added to this index
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ProjectIndexer_GatheringNodeData(object sender, IndexingNodeDataEventArgs e)
        {
            //Need to add category, which is a parent folder if it has one, we only care about published data
            // so we can just look this up from the published cache

            //TODO: To get at the IPublishedCaches it is only available on the UmbracoContext (which we need to fix)
            // but since this method operates async, there isn't one, so we need to make our own to get at the cache
            // object by creating a fake HttpContext. Not pretty but it works for now.
            if (UmbracoContext.Current == null)
            {
                var dummyHttpContext = new HttpContextWrapper(
                    new HttpContext(
                        new SimpleWorkerRequest("blah.aspx", "", new StringWriter())));
                UmbracoContext.EnsureContext(dummyHttpContext,
                    ApplicationContext.Current,
                    new WebSecurity(dummyHttpContext, ApplicationContext.Current), false);    
            }
            
            var node = UmbracoContext.Current.ContentCache.GetById(e.NodeId);
            if (node == null) return;

            //this has a project group which is it's category
            if (node.Parent.DocumentTypeAlias == "ProjectGroup")
            {
                e.Fields["categoryFolder"] = node.Parent.Name;
            }


        }

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            CreateRoutes();
            BindExamineEvents();
        }
    }
}
