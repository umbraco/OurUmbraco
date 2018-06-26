using System;
using System.Net;
using System.Web;
using Examine;
using Examine.LuceneEngine.Providers;
using ImageProcessor.Web.HttpModules;
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
            ImageProcessingModule.ValidatingRequest += ImageProcessingModule_ValidatingRequest;
        }

        private void ImageProcessingModule_ValidatingRequest(object sender, ImageProcessor.Web.Helpers.ValidatingRequestEventArgs e)
        {
            // Nothing to process, return immediately
            if (string.IsNullOrWhiteSpace(e.QueryString))
                return;

            var isGif = e.Context.Request.Path.EndsWith(".gif", StringComparison.CurrentCultureIgnoreCase);
            if (isGif == false)
                return;

            // Don't support processing gifs
            e.QueryString = "";
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
    }
}
