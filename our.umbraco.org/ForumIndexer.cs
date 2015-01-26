using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

//WB Added
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using our;
using umbraco.BusinessLogic;

namespace our
{
    public class ForumIndexer : ApplicationBase
    {
        public ForumIndexer()
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
            var dataSet = ((CustomDataService)indexer.DataService).CreateNewDocument(e.Topic.Id);
            var xml = dataSet.RowData.ToExamineXml(dataSet.NodeDefinition.NodeId, dataSet.NodeDefinition.Type);
            indexer.ReIndexNode(xml, "documents");
        }

    }

}

