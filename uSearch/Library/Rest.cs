using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Xml.XPath;
using Examine;
using Examine.Providers;
using Examine.SearchCriteria;
using Lucene.Net.Search;
//using Marketplace.Interfaces;
//using Marketplace.Providers;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.presentation;
using Umbraco.Web.BaseRest;
using UmbracoExamine;

namespace uSearch.Library
{
    [RestExtension("uSearch")]
    public class Rest
    {
        public static XPathNodeIterator FindSimiliarItems(string types, int maxItems)
        {
            string q = umbraco.library.StripHtml(HttpContext.Current.Request["q"]);
            string keywords = string.Join(" ", Xslt.GetKeywords(q)).Trim();
            return Xslt.LuceneInContentType(keywords, types, 0, 255, maxItems);
        }

        [RestExtensionMethod(ReturnXml = false)]
        public static string FindProjects(string query, int parent, bool wildcard)
        {
            if (query.ToLower() == "useqsstring") query = UmbracoContext.Current.Request.QueryString["term"];
            if (wildcard && !query.EndsWith("*")) query += "*";
            string searchTerm = query;
            BaseSearchProvider searcher = ExamineManager.Instance.SearchProviderCollection["MultiIndexSearcher"];



            //Search Criteria for WIKI & Projects
            var searchCriteria = searcher.CreateSearchCriteria(BooleanOperation.Or);
            var searchQuery = searchTerm.BuildExamineString(99, "nodeName");
            searchQuery += searchTerm.BuildExamineString(10, "description");
            searchQuery = "(" + searchQuery + ") AND +approved:1";
            var searchFilter = searchCriteria.RawQuery(searchQuery);
            IEnumerable<SearchResult> searchResults = searcher.Search(searchFilter).OrderByDescending(x => x.Score);
            searchResults = from r in searchResults
                            where r["__IndexType"] == "content" && r["nodeTypeAlias"] == "Project"
                            select r;
                            //orderby int.Parse(r["downloads"]) descending select r;

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(searchResults);
        }


    }

    public class ProjectExamineIndexer : ApplicationBase
    {
        public ProjectExamineIndexer()
        {
            var indexer = ExamineManager.Instance.IndexProviderCollection["ProjectIndexer"];

            // intercept when a project is indexed to add downloads/karma stats
            indexer.GatheringNodeData += new EventHandler<IndexingNodeDataEventArgs>(indexer_GatheringNodeData);

            uPowers.BusinessLogic.Action.AfterPerform += new EventHandler<uPowers.BusinessLogic.ActionEventArgs>(Action_AfterPerform);

        }

        void Action_AfterPerform(object sender, uPowers.BusinessLogic.ActionEventArgs e)
        {
            if (e.ActionType == "project")
            {
                Log.Add(LogTypes.Debug, int.Parse(e.ItemId.ToString()), "Karma indexing starts");
                try
                {
                    ExamineManager.Instance.IndexProviderCollection["ProjectIndexer"].ReIndexNode(new Document(e.ItemId).ToXDocument(true).Root, IndexTypes.Content);
                }
                catch (Exception ee)
                {
                    Log.Add(LogTypes.Debug, int.Parse(e.ItemId.ToString()), "Karma indexing failed " + ee.ToString());
                }
            }
        }

        void indexer_GatheringNodeData(object sender, IndexingNodeDataEventArgs e)
        {
            //try
            //{
            //    if (e.Fields["nodeTypeAlias"] == "Project")
            //    {
            //        var projectsProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            //        var project = projectsProvider.GetListing(e.NodeId, true);

            //        if (project != null)
            //        {
            //            // add downloads
            //            e.Fields["downloads"] = project.Downloads.ToString();

            //            // add karma
            //            e.Fields["karma"] = project.Karma.ToString();

            //            // add unique id (needed by repo)
            //            e.Fields["uniqueId"] = project.ProjectGuid.ToString();

            //            // add category
            //            e.Fields["categoryId"] = project.CategoryId.ToString();
            //            e.Fields["category"] = Marketplace.library.GetCategoryName(project.Id);

            //            Log.Add(LogTypes.Debug, e.NodeId, "Done adding karma/download data to project index");
            //        }

            //    }
            //}
            //catch (Exception ee)
            //{
            //    Log.Add(LogTypes.Debug, e.NodeId, string.Format("Error adding data to project index: {0}", ee));
            //}
        }
    }

    public static class ExamineHelpers
    {
        public static string BuildExamineString(this string term, int boost, string field)
        {
            var terms = term.Split(' ');
            var qs = field + ":";
            qs += "\"" + term + "\"^" + (boost + 30000).ToString() + " ";
            qs += field + ":(+" + term.Replace(" ", " +") + ")^" + (boost + 5).ToString() + " ";
            qs += field + ":(" + term + ")^" + boost.ToString() + " ";
            return qs;
        }

    }
}
