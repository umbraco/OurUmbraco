using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

//WB Added
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using uForum.Businesslogic;
using our;
using umbraco.BusinessLogic;

namespace our
{
    public class ForumIndexer : ApplicationBase
    {
        public ForumIndexer()
        {
            //WB added to show these events are firing...
            Log.Add(LogTypes.Debug, -1, "ForumIndexer class events - starting");

            //WB 17/4/11 - Comment out events to see if this fixes karma points & email problems
            
            Topic.AfterCreate += new EventHandler<CreateEventArgs>(Topic_AfterCreate);
            Topic.AfterUpdate += new EventHandler<UpdateEventArgs>(Topic_AfterUpdate);
            Topic.BeforeDelete += new EventHandler<DeleteEventArgs>(Topic_BeforeDelete);
            

            //WB added to show these events have finished firing...
            Log.Add(LogTypes.Debug, -1, "ForumIndexer class events - finished");
        }

        void Topic_BeforeDelete(object sender, DeleteEventArgs e)
        {            
            Topic t = (Topic)sender;

            var indexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["ForumIndexer"];
            indexer.DeleteFromIndex(t.Id.ToString());
        }

        static void Topic_AfterUpdate(object sender, UpdateEventArgs e)
        {
            Topic currentTopic = (Topic)sender;

            //WB added to show this event is firing...
            Log.Add(LogTypes.Debug, currentTopic.Id, "Topic_AfterUpdate in ForumIndexer() class is starting");
            
            var indexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["ForumIndexer"];
            var dataSet = ((CustomDataService)indexer.DataService).CreateNewDocument(currentTopic.Id);
            var xml = dataSet.RowData.ToExamineXml(dataSet.NodeDefinition.NodeId, dataSet.NodeDefinition.Type);

            indexer.ReIndexNode(xml, "documents");

            //WB added to show this event is firing...
            Log.Add(LogTypes.Debug, currentTopic.Id, "Topic_AfterUpdate in ForumIndexer() class is finishing");
        }

        static void Topic_AfterCreate(object sender, CreateEventArgs e)
        {
            Topic currentTopic = (Topic)sender;

            //WB added to show this event is firing...
            Log.Add(LogTypes.Debug, currentTopic.Id, "Topic_AfterCreate in ForumIndexer() class is starting");
           
            var indexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["ForumIndexer"];
            var dataSet = ((CustomDataService)indexer.DataService).CreateNewDocument(currentTopic.Id);
            var xml = dataSet.RowData.ToExamineXml(dataSet.NodeDefinition.NodeId, dataSet.NodeDefinition.Type);
            
            indexer.ReIndexNode(xml, "documents");

            //WB added to show this event is firing...
            Log.Add(LogTypes.Debug, currentTopic.Id, "Topic_AfterCreate in ForumIndexer() class is finishing");
        }


    }

}

