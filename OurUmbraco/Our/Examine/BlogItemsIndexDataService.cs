using System;
using System.Collections.Generic;
using Examine;
using Examine.LuceneEngine;
using OurUmbraco.Community.BlogPosts;

namespace OurUmbraco.Our.Examine
{
    /// <summary>
    /// Used to index the documentation 
    /// </summary>
    public class BlogItemsIndexDataService : ISimpleDataService
    {

        public IEnumerable<SimpleDataSet> GetAllData(string indexType)
        {
            var service = new BlogPostsService();

            var items = service.GetBlogPosts();

            foreach (var item in items)
            {
                var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };
                simpleDataSet = MapBlogItemToSimpleDataIndexItem(item, simpleDataSet, indexType);
                yield return simpleDataSet;
            }

        }
        public static void ReIndex(BlogDatabaseItem item)
        {

            var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };

            var examineNode = MapBlogItemToSimpleDataIndexItem(item, simpleDataSet, "blogItems").RowData.ToExamineXml(item.Id, "blogItems");

            ExamineManager.Instance.IndexProviderCollection["BlogItemsIndexer"].ReIndexNode(examineNode, "blogItems");

        }


        public static SimpleDataSet MapBlogItemToSimpleDataIndexItem(BlogDatabaseItem item, SimpleDataSet simpleDataSet, string indexType)
        {

            simpleDataSet.NodeDefinition.NodeId = item.Id;
            simpleDataSet.NodeDefinition.Type = indexType;

            simpleDataSet.RowData.Add("body", String.Empty);
            simpleDataSet.RowData.Add("nodeName", ExamineHelper.RemoveSpecialCharacters(item.Title));
            simpleDataSet.RowData.Add("nodeTypeAlias", "blogItem");

            simpleDataSet.RowData.Add("createDate", item.PublishedDate.ToString("yyyy-MM-dd HH:mm:ss"));
            simpleDataSet.RowData.Add("updateDate", item.PublishedDate.ToString("yyyy-MM-dd HH:mm:ss"));

            simpleDataSet.RowData.Add("blogId", item.BlogId);
            simpleDataSet.RowData.Add("data", item.DataRaw);
            simpleDataSet.RowData.Add("url", item.Data.Link);

            return simpleDataSet;

        }


    }
}