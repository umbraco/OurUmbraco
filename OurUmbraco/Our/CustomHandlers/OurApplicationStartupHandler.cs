using System.IO;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Examine;
using Examine.LuceneEngine.Providers;
using our.Examine;
using OurUmbraco.Documentation.Busineslogic;
using OurUmbraco.Documentation.Busineslogic.GithubSourcePull;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace our.CustomHandlers
{
    /// <summary>
    /// Main Application startup handler
    /// </summary>
    public class OurApplicationStartupHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            CreateRoutes();
            BindExamineEvents();
            ZipDownloader.OnFinish += ZipDownloader_OnFinish;
        }

        private void CreateRoutes()
        {
            RouteTable.Routes.MapUmbracoRoute("Search", "search/{term}",
                //NOTE: Even though we aren't routing to a 'controller', this syntax is required for the route to be registered in the table
                new { Controller = "Search", Action = "Search", Term = UrlParameter.Optional },
                //NOTE: This virtual page will be routed as if it were the root 'community' page
                new SearchPageRouteHandler(1052));
        }

        private void BindExamineEvents()
        {
            var projectIndexer = (LuceneIndexer)ExamineManager.Instance.IndexProviderCollection["projectIndexer"];
            projectIndexer.GatheringNodeData += ProjectNodeIndexDataService.ProjectIndexer_GatheringNodeData;
            projectIndexer.DocumentWriting += ProjectNodeIndexDataService.ProjectIndexer_DocumentWriting;

            //handle errors for non-umbraco indexers
            ExamineManager.Instance.IndexProviderCollection["projectIndexer"].IndexingError += ExamineHelper.LogErrors;
            ExamineManager.Instance.IndexProviderCollection["documentationIndexer"].IndexingError += ExamineHelper.LogErrors;
            ExamineManager.Instance.IndexProviderCollection["ForumIndexer"].IndexingError += ExamineHelper.LogErrors;
        }


        /// <summary>
        /// Whenever the github zip downloader completes and docs index is rebuilt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ZipDownloader_OnFinish(object sender, FinishEventArgs e)
        {
            var indexer = ExamineManager.Instance.IndexProviderCollection[ExamineHelper.DocumentationIndexer];

            //TODO: Fix this - we cannot "Rebuild" on a live site, because the entire index will be taken down/deleted and then recreated, if people
            // are searching during this operation, YSODs will occur.
            indexer.RebuildIndex();
        }
    }
}
