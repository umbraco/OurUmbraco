using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using our.Examine;
using Umbraco.Core;
//WB Added


namespace our.CustomHandlers
{
    public class ForumIndexer : ApplicationEventHandler
    {
     
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            uForum.Services.TopicService.Created += TopicService_Updated;
            uForum.Services.TopicService.Updated += TopicService_Updated;
            uForum.Services.TopicService.Deleting += TopicService_Deleted;
        }


        void TopicService_Deleted(object sender, uForum.TopicEventArgs e)
        {
            var indexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["ForumIndexer"];
            indexer.DeleteFromIndex(e.Topic.Id.ToString());
        }

        void TopicService_Updated(object sender, uForum.TopicEventArgs e)
        {
            var indexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["ForumIndexer"];
            var dataSet = ((ForumDataService)indexer.DataService).CreateNewDocument(e.Topic.Id);
            var xml = dataSet.RowData.ToExamineXml(dataSet.NodeDefinition.NodeId, dataSet.NodeDefinition.Type);
            indexer.ReIndexNode(xml, "forum");
        }

    }

}

