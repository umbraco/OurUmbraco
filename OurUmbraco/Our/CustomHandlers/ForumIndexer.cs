using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using OurUmbraco.Forum;
using OurUmbraco.Forum.Services;
using OurUmbraco.Our.Examine;
using Umbraco.Core;

//WB Added


namespace OurUmbraco.Our.CustomHandlers
{
    public class ForumIndexer : ApplicationEventHandler
    {

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            TopicService.Created += TopicService_Updated;
            TopicService.Updated += TopicService_Updated;
            TopicService.Deleting += TopicService_Deleted;
        }
        
        void TopicService_Deleted(object sender, TopicEventArgs e)
        {
            var indexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["ForumIndexer"];
            indexer.DeleteFromIndex(e.Topic.Id.ToString());
        }

        void TopicService_Updated(object sender, TopicEventArgs e)
        {
            var indexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["ForumIndexer"];
            if (e.Topic.IsSpam)
            {
                indexer.DeleteFromIndex(e.Topic.Id.ToString());
            }
            else
            {
                var dataSet = ((ForumDataService)indexer.DataService).CreateNewDocument(e.Topic.Id);
                var xml = dataSet.RowData.ToExamineXml(dataSet.NodeDefinition.NodeId, dataSet.NodeDefinition.Type);
                indexer.ReIndexNode(xml, "forum");
            }
        }

    }

}

