﻿using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using our.Examine;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using Umbraco.Web;
using uWiki.Businesslogic;

namespace our.CustomHandlers
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
                ((SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["projectIndexer"]).DeleteFromIndex(item.Id.ToString());
        }

        void ContentService_Published(Umbraco.Core.Publishing.IPublishingStrategy sender, Umbraco.Core.Events.PublishEventArgs<IContent> e)
        {
            foreach (var item in e.PublishedEntities.Where(x => x.ContentType.Alias == "Project"))
            {
                if (item.GetValue<bool>("projectLive"))
                {
                    UpdateProjectExamineIndex(item);
                }
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
            var simpleDataSet = new SimpleDataSet
            {
                NodeDefinition = new IndexedNode(),
                RowData = new Dictionary<string, string>()
            };

            var karma = Utils.GetProjectTotalVotes(content.Id);
            var files = WikiFile.CurrentFiles(content.Id);
            var compatVersions = Utils.GetProjectCompatibleVersions(content.Id);

            var simpleDataIndexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["projectIndexer"];
            simpleDataSet = ((ProjectNodeIndexDataService)simpleDataIndexer.DataService)
                .MapProjectToSimpleDataIndexItem(content, simpleDataSet, "project", karma, files, downloads, compatVersions);

            var xml = simpleDataSet.RowData.ToExamineXml(simpleDataSet.NodeDefinition.NodeId, simpleDataSet.NodeDefinition.Type);
            simpleDataIndexer.ReIndexNode(xml, "project");
        }
    }
}
