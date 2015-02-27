using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using umbraco.BusinessLogic;

using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using our;
using our.Examine;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace our
{
    public class ProjectIndexer : ApplicationBase
    {
        public ProjectIndexer()
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
            var indexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["ForumIndexer"];
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            
            foreach (var item in e.PublishedEntities.Where(x => x.ContentType.Alias == "Project"))
            {
                var content = umbracoHelper.TypedContent(item.Id);
                var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };

                var karma = our.Utils.GetProjectTotalKarma(content.Id);
                var files = uWiki.Businesslogic.WikiFile.CurrentFiles(content.Id);
                var downloads = our.Utils.GetProjectTotalDownloadCount(content.Id);

                simpleDataSet = ((ProjectNodeIndexDataService) indexer.DataService).MapProjectToSimpleDataIndexItem(content, simpleDataSet, "project", karma, files, downloads);

                var xml = simpleDataSet.RowData.ToExamineXml(simpleDataSet.NodeDefinition.NodeId, simpleDataSet.NodeDefinition.Type);
                indexer.ReIndexNode(xml, "project");
            }
        }

        

    }

}
