using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using Examine;
using Examine.LuceneEngine;
using OurUmbraco.Community.BlogPosts;
using OurUmbraco.Documentation.Busineslogic.GithubSourcePull;

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

            var items = service.GetAllBlogItemsFromDatabase();

            int i = 0;

            foreach (var item in items)
            {
                i++;
                var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };
                simpleDataSet = MapBlogItemToSimpleDataIndexItem(item, simpleDataSet, i, indexType);
                yield return simpleDataSet;
            }

        }


        public static SimpleDataSet MapBlogItemToSimpleDataIndexItem(BlogDatabaseItem item, SimpleDataSet simpleDataSet, int index, string indexType)
        {

            simpleDataSet.NodeDefinition.NodeId = index;
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