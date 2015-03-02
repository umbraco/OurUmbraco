using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine;
using Examine.LuceneEngine;
using uForum.Models;
using uForum.Services;
//WB Added

using uForum;
using Umbraco.Core;

namespace our.Examine
{
   
    /// <summary>
    /// The data service used by the LuceneEngine in order for it to reindex all data
    /// </summary>
    public class ForumDataService : ISimpleDataService
    {

        public static SimpleDataSet MapTopicToSimpleDataIndexItem(ReadOnlyTopic topic, SimpleDataSet simpleDataSet, int id, string indexType)
        {
            //First generate the accumulated comment text:
            string commentText = String.Empty;

            //TODO: I don't think this is a great idea... the body should remain as-is and the comments should be concat'd to another field
            // This would allow us to boost correctly for the body field
            foreach (var currentComment in topic.Comments)
                commentText += currentComment.Body;

            var body = umbraco.library.StripHtml(topic.Body + commentText);

            simpleDataSet.NodeDefinition.NodeId = id;
            simpleDataSet.NodeDefinition.Type = indexType;

            simpleDataSet.RowData.Add("body", body);
            simpleDataSet.RowData.Add("nodeName", topic.Title);
            simpleDataSet.RowData.Add("updateDate", topic.Updated.ToString("yyyy-MM-dd HH:mm:ss"));
            simpleDataSet.RowData.Add("nodeTypeAlias", "forum");
            simpleDataSet.RowData.Add("url", topic.Url);

            simpleDataSet.RowData.Add("createDate", topic.Created.ToString("yyyy-MM-dd HH:mm:ss"));

            simpleDataSet.RowData.Add("latestCommentId", topic.LatestComment.ToString());
            simpleDataSet.RowData.Add("latestReplyAuthorId", topic.LatestReplyAuthor.ToString());
            simpleDataSet.RowData.Add("latestReplyAuthorName", topic.LastReplyAuthorName);

            simpleDataSet.RowData.Add("authorId", topic.MemberId.ToString());
            simpleDataSet.RowData.Add("authorName", topic.AuthorName);
            
            simpleDataSet.RowData.Add("parentId", topic.ParentId.ToString());
            simpleDataSet.RowData.Add("replies", topic.Replies.ToString());

            simpleDataSet.RowData.Add("locked", topic.Locked.ToString());
            simpleDataSet.RowData.Add("solved", topic.Answer.ToString());

            simpleDataSet.RowData.Add("version", topic.Version.ToString());

            return simpleDataSet;
        }

        public IEnumerable<SimpleDataSet> GetAllData(string indexType)
        {
            var data = new List<SimpleDataSet>();

            var ts = new TopicService(ApplicationContext.Current.DatabaseContext);

            foreach (var topic in ts.QueryAll())
            {
                //Add the item to the index..
                var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };
                data.Add(MapTopicToSimpleDataIndexItem(topic, simpleDataSet, topic.Id, "forum"));
            }

            return data;
            
        }


        public SimpleDataSet CreateNewDocument(int id)
        {
            var ts = new TopicService(ApplicationContext.Current.DatabaseContext);           

            var forumTopic = ts.QueryById(id);
            var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };
            return MapTopicToSimpleDataIndexItem(forumTopic, simpleDataSet, forumTopic.Id, "forum");
        }



    }
}


