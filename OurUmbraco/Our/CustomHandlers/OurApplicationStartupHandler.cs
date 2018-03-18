using System.Net;
using Examine;
using Examine.LuceneEngine.Providers;
using OurUmbraco.Documentation.Busineslogic;
using OurUmbraco.Documentation.Busineslogic.GithubSourcePull;
using OurUmbraco.Our.Controllers;
using OurUmbraco.Our.Examine;
using Umbraco.Core;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.CustomHandlers
{
    /// <summary>
    /// Main Application startup handler
    /// </summary>
    public class OurApplicationStartupHandler : ApplicationEventHandler
    {        

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            BindExamineEvents();
            ZipDownloader.OnFinish += ZipDownloader_OnFinish;
        }

        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            DefaultRenderMvcControllerResolver.Current.SetDefaultControllerType(typeof(OurUmbracoController));
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol = 
                SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
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
            ExamineManager.Instance.IndexProviderCollection["PullRequestIndexer"].IndexingError += ExamineHelper.LogErrors;
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
