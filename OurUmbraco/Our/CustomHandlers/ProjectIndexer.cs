using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using OurUmbraco.Our.Examine;
using OurUmbraco.Wiki.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace OurUmbraco.Our.CustomHandlers
{
    public class ProjectIndexer : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Published += ContentService_Published;
            ContentService.Deleted += ContentService_Deleted;
            WikiFile.AfterDownloadUpdate += WikiFile_AfterDownloadUpdate;
        }

        void ContentService_Deleted(IContentService sender, Umbraco.Core.Events.DeleteEventArgs<IContent> e)
        {
            foreach (var item in e.DeletedEntities.Where(x => x.ContentType.Alias == "Project"))
                ExamineManager.Instance.IndexProviderCollection["projectIndexer"].DeleteFromIndex(item.Id.ToString());
        }

        void ContentService_Published(Umbraco.Core.Publishing.IPublishingStrategy sender, Umbraco.Core.Events.PublishEventArgs<IContent> e)
        {
            foreach (var item in e.PublishedEntities.Where(x => x.ContentType.Alias == "Project"))
            {
                UpdateProjectExamineIndex(item);
            }
        }

        void WikiFile_AfterDownloadUpdate(object sender, FileDownloadUpdateEventArgs e)
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var content = umbracoHelper.TypedContent(e.ProjectId);
            UpdateProjectExamineIndex(content, e.Downloads);
        }

        private void UpdateProjectExamineIndex(IEntity item)
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var content = umbracoHelper.TypedContent(item.Id);
            var downloads = Utils.GetProjectTotalDownloadCount(content.Id);
            UpdateProjectExamineIndex(content, downloads);
        }

        private void UpdateProjectExamineIndex(IPublishedContent content, int downloads)
        {
            if (content == null)
                return;

            var simpleDataSet = new SimpleDataSet
            {
                NodeDefinition = new IndexedNode(),
                RowData = new Dictionary<string, string>()
            };

            var projectVotes = Utils.GetProjectTotalVotes(content.Id);
            var files = WikiFile.CurrentFiles(content.Id).ToArray();
            var compatVersions = Utils.GetProjectCompatibleVersions(content.Id) ?? new List<string>();
            var downloadStats = WikiFile.GetMonthlyDownloadStatsByProject(
                content.Id,
                DateTime.Now.Subtract(TimeSpan.FromDays(365)));

            var nugetService = new OurUmbraco.Community.Nuget.NugetPackageDownloadService();
            var nugetPackageId = nugetService.GetNuGetPackageId(content);
            int? dailyNugetDownLoads = null;


            if (!nugetPackageId.IsNullOrWhiteSpace())
            {
                var nugetDownloads = nugetService.GetNugetPackageDownloads();

                var packageInfo = nugetDownloads.FirstOrDefault(x => x.PackageId == nugetPackageId);

                if (packageInfo != null)
                {
                    downloads += packageInfo.TotalDownLoads;
                    dailyNugetDownLoads = packageInfo.AverageDownloadPerDay;
                }
            }


            var simpleDataIndexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["projectIndexer"];
            simpleDataSet = ((ProjectNodeIndexDataService)simpleDataIndexer.DataService)
                .MapProjectToSimpleDataIndexItem(downloadStats, DateTime.Now, content, simpleDataSet, "project", projectVotes, files, downloads, compatVersions, dailyNugetDownLoads);

            if (simpleDataSet.NodeDefinition.Type == null)
                simpleDataSet.NodeDefinition.Type = "project";

            var xml = simpleDataSet.RowData.ToExamineXml(simpleDataSet.NodeDefinition.NodeId, simpleDataSet.NodeDefinition.Type);
            simpleDataIndexer.ReIndexNode(xml, "project");
        }
    }
}
