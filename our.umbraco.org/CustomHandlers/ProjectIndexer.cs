using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using our.Examine;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace our.CustomHandlers
{
    public class ProjectIndexer : ApplicationEventHandler
    {
       
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Published += ContentService_Published;
            ContentService.Deleted += ContentService_Deleted;
        }

        void ContentService_Deleted(IContentService sender, Umbraco.Core.Events.DeleteEventArgs<Umbraco.Core.Models.IContent> e)
        {
            
            var indexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["projectIndexer"];
            foreach (var item in e.DeletedEntities.Where(x => x.ContentType.Alias == "Project"))
                indexer.DeleteFromIndex(item.Id.ToString());
        }

        void ContentService_Published(Umbraco.Core.Publishing.IPublishingStrategy sender, Umbraco.Core.Events.PublishEventArgs<Umbraco.Core.Models.IContent> e)
        {
            var indexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["projectIndexer"];
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            
            foreach (var item in e.PublishedEntities.Where(x => x.ContentType.Alias == "Project"))
            {
                if (item.GetValue<bool>("projectLive"))
                {
                    var content = umbracoHelper.TypedContent(item.Id);
                    var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };

                    var karma = Utils.GetProjectTotalVotes(content.Id);
                    var files = uWiki.Businesslogic.WikiFile.CurrentFiles(content.Id);
                    var downloads = Utils.GetProjectTotalDownloadCount(content.Id);
                    var compatVersions = Utils.GetProjectCompatibleVersions(content.Id);

                    simpleDataSet = ((ProjectNodeIndexDataService)indexer.DataService).MapProjectToSimpleDataIndexItem(
                        content, simpleDataSet, "project", karma, files, downloads, compatVersions);

                    var xml = simpleDataSet.RowData.ToExamineXml(simpleDataSet.NodeDefinition.NodeId, simpleDataSet.NodeDefinition.Type);
                    indexer.ReIndexNode(xml, "project");
                }
            }
        }

        

    }

}
