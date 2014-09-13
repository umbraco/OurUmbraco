using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Web.UI;
using System.Xml.XPath;
using System.Text.RegularExpressions;


using Examine;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using System.Data.SqlClient;
using Examine.SearchCriteria;
using Examine.LuceneEngine.SearchCriteria;

namespace uSearch.Library {
    public class Xslt {


        public static string Reindex(bool async)
        {
            Businesslogic.Indexer i = new uSearch.Businesslogic.Indexer();
            if (async)
                i.AsyncReindex();
            else
                i.ReIndex();

            return "";
        }


        public static string ReIndexVideo()
        {
            Businesslogic.Indexer i = new Businesslogic.Indexer();
            //new EventHandlers.Video().Indexer_AfterReIndex(i, new Businesslogic.ReIndexEventArgs());

            return "";
        }

        public static string ReIndexWiki()
        {
            Businesslogic.Indexer i = new Businesslogic.Indexer();
            new EventHandlers.WikiHandler().Indexer_AfterReIndex(i, new Businesslogic.ReIndexEventArgs());

            return "";
        }

        public static string ReIndexProjects()
        {
            Businesslogic.Indexer i = new Businesslogic.Indexer();
            new EventHandlers.ProjectHandler().Indexer_AfterReIndex(i, new Businesslogic.ReIndexEventArgs());

            return "";
        }

        public static string ReIndexForum()
        {
            Businesslogic.Indexer i = new Businesslogic.Indexer();

            new EventHandlers.ForumHandler().Indexer_AfterReIndex(i, new Businesslogic.ReIndexEventArgs());

            return "";
        }

        public static string[] GetKeywords(string text)
        {
            string[] stop = { "about", "after", "all", "also", "an", "and", "another", "any", "are", "as", "at", "be", "because", "been", "before", "being", "between", "both", "but", "by", "came", "can", "come", "could", "did", "do", "does", "each", "else", "for", "from", "get", "got", "has", "had", "he", "have", "her", "here", "him", "himself", "his", "how", "i", "if", "in", "into", "is", "it", "its", "just", "like", "make", "many", "me", "might", "more", "most", "much", "must", "my", "never", "now", "of", "on", "only", "or", "other", "our", "out", "over", "re", "said", "same", "see", "should", "since", "so", "some", "still", "such", "take", "than", "that", "the", "their", "them", "then", "there", "these", "they", "this", "those", "through", "to", "too", "under", "up", "use", "very", "want", "was", "way", "we", "well", "were", "what", "when", "where", "which", "while", "who", "will", "with", "would", "you", "your", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "$", "£", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };

            char[] splitChars = { ' ', '\'' };
            string[] words = text.Split(splitChars);

            var keywordCount = (from keyword in words.Except(stop)
                                group keyword by keyword into g
                                select new { Keyword = g.Key, Count = g.Count() });
            return keywordCount.OrderByDescending(k => k.Count).Select(k => k.Keyword).Take(5).ToArray();
        }

        public static string BuildExamineString(string term, int boost, string field)
        {
            var terms = term.Split(' ');
            var qs = field + ":";
            qs += "\"" + term + "\"^" + (boost + 30000).ToString() + " ";
            qs += field + ":(+" + term.Replace(" ", " +") + ")^" + (boost + 5).ToString() + " ";
            qs += field + ":(" + term + ")^" + boost.ToString() + " ";
            return qs;
        }

        //convert ID to lucene friendly path eg: 1123 -> 1234s1232s1123s
        public static string LucenePath(int nodeId) {
            umbraco.presentation.nodeFactory.Node umbNode = new umbraco.presentation.nodeFactory.Node(nodeId);
            string path = "";
            if (umbNode != null) {
                path = umbNode.Path.Replace("-1,", "").Replace(",", new Businesslogic.Settings().PathSplit);
            }
            return path;
        }

        public static XPathNodeIterator LucenePager(int currentPage, int totalPages) {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode root = xmlDoc.CreateElement("pages");
            xmlDoc.AppendChild(root);

            if (currentPage > totalPages)
                currentPage = totalPages;

            for (int i = 0; i < totalPages; i++) {
                XmlNode page = xmlDoc.CreateElement("page");
                page.Attributes.Append(umbraco.xmlHelper.addAttribute(xmlDoc, "index", i.ToString()));
                if (i == currentPage)
                    page.Attributes.Append(umbraco.xmlHelper.addAttribute(xmlDoc, "current", "true"));

                root.AppendChild(page);
            }

            return xmlDoc.CreateNavigator().Select(".");
        }


        //private static Query GetSafeQuery(MultiFieldQueryParser qp, String query) {
        //    Query q;
        //    try {
        //        q = qp.Parse(query);
        //    } catch (Lucene.Net.QueryParsers.ParseException e) {
        //        q = null;
        //    }

        //    if (q == null) {
        //        string cooked;

        //        cooked = Regex.Replace(query, @"[^\w\.@-]", " ");
        //        q = qp.Parse(cooked);
        //    }

        //    return q;
        //}


        public static XmlDocument Lucene(string q, int currentPage, int trimAtChar, int pagesize)
        {
            return LuceneInContentType(q, string.Empty, currentPage, trimAtChar, pagesize);
        }

        public static XmlDocument LuceneInContentType(string q, string types, int currentPage, int trimAtChar, int pagesize)
        {
            int _pageSize = pagesize;
            int _page = currentPage;
            int _fragmentCharacters = trimAtChar;
            int _googleNumbers = 10;

            int _TotalResults;

            // Enable next, parent, headers and googleArrows
            DateTime searchStart = DateTime.Now;

            string queryText = q;
            string[] _fields = {"name","content"};


            q = q.Replace("-", "\\-");

            new Page().Trace.Write("umbAdvancedSearch:doSearch", "Performing search! using query " + q);


             BaseSearchProvider Searcher = ExamineManager.Instance.SearchProviderCollection["ForumSearcher"];
            var searchCriteria = Searcher.CreateSearchCriteria(BooleanOperation.Or);
            var searchQuery = BuildExamineString(queryText,10, "Title");
            searchQuery += BuildExamineString(queryText,7, "CommentsContent").TrimEnd(' ');

            var searchFilter = searchCriteria.RawQuery(searchQuery);

            IEnumerable<SearchResult> searchResults;

            //little hacky just to get performant searching
            if (pagesize > 0)
            {
                searchResults = Searcher.Search(searchFilter)
                .OrderByDescending(x => x.Score).Take(pagesize);
            }
            else
            {
                searchResults = Searcher.Search(searchFilter)
                .OrderByDescending(x => x.Score);
            }



            _TotalResults = searchResults.Count();
            TimeSpan searchEnd = DateTime.Now.Subtract(searchStart);
            string searchTotal = searchEnd.Seconds + ".";
            for (int i = 4; i > searchEnd.Milliseconds.ToString().Length; i--)
                searchTotal += "0";
            searchTotal += searchEnd.Milliseconds.ToString();
                                   
            
            // Check for paging
            int pageSize = _pageSize;
            int pageStart = _page;
            if (pageStart > 0)
                pageStart = _page * pageSize;
            int pageEnd = (_page + 1) * pageSize;



            //calculating total items and number of pages...
            int _firstGooglePage = _page - Convert.ToInt16(_googleNumbers / 2);
            if (_firstGooglePage < 0)
                _firstGooglePage = 0;
            int _lastGooglePage = _firstGooglePage + _googleNumbers;

            if (_lastGooglePage * pageSize > _TotalResults)
            {
                _lastGooglePage = (int)Math.Ceiling(_TotalResults / ((double)pageSize)); // Convert.ToInt32(hits.Length()/pageSize)+1;

                _firstGooglePage = _lastGooglePage - _googleNumbers;
                if (_firstGooglePage < 0)
                    _firstGooglePage = 0;
            }

            // Create xml document
            XmlDocument xd = new XmlDocument();
            xd.LoadXml("<search/>");
            xd.DocumentElement.AppendChild(umbraco.xmlHelper.addCDataNode(xd, "query", queryText));
            xd.DocumentElement.AppendChild(umbraco.xmlHelper.addCDataNode(xd, "luceneQuery", q));
            xd.DocumentElement.AppendChild(umbraco.xmlHelper.addTextNode(xd, "results", _TotalResults.ToString()));
            xd.DocumentElement.AppendChild(umbraco.xmlHelper.addTextNode(xd, "currentPage", _page.ToString()));
            xd.DocumentElement.AppendChild(umbraco.xmlHelper.addTextNode(xd, "totalPages", _lastGooglePage.ToString()));

            XmlNode results = umbraco.xmlHelper.addTextNode(xd, "results", "");
            xd.DocumentElement.AppendChild(results);

            int highlightFragmentSizeInBytes = _fragmentCharacters;

            // Print results
            new Page().Trace.Write("umbSearchResult:doSearch", "printing results using start " + pageStart + " and end " + pageEnd);

            int r = 0;
            foreach (var sr in searchResults.Skip(pageStart).Take(pageSize))
            {


                XmlNode result = xd.CreateNode(XmlNodeType.Element, "result", "");
                result.AppendChild(umbraco.xmlHelper.addTextNode(xd, "score", (sr.Score * 100).ToString()));
                result.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "resultNumber", (r + 1).ToString()));

                foreach (var field in sr.Fields)
                {
                    result.AppendChild(umbraco.xmlHelper.addTextNode(xd, field.Key, field.Value));

                }

                results.AppendChild(result);
                r++;

            }

            return xd;
        } //end search

    }

}
