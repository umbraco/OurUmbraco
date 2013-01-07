using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using System.Data.SqlClient;
using Examine.SearchCriteria;
using Examine.LuceneEngine.SearchCriteria;

namespace our.usercontrols
{
    public static class SearchResultExtensions
    {
        public static string getTitle(this SearchResult result)
        {
            if (result["__IndexType"] == "content")
                return HttpContext.Current.Server.HtmlEncode(result["nodeName"]);
            else if (result["__IndexType"] == "documents" || result["__IndexType"] == "documentation")
                return HttpContext.Current.Server.HtmlEncode(result["Title"]);
            else
                return string.Empty;

        }

        public static string fullURL(this SearchResult result)
        {
            if (result["__IndexType"] == "content")
                return umbraco.library.NiceUrl(result.Id);

            else if (result["__IndexType"] == "documentation")
            {
                return result["url"];
            }
            else if (result["__IndexType"] == "documents")
                return uForum.Library.Xslt.NiceTopicUrl(result.Id);
            else
                return string.Empty;
        }

        public static string generateBlurb(this SearchResult result, int noOfChars)
        {
            var text = string.Empty;

            if (result["__IndexType"] == "content")
            {
                switch (result.Fields["nodeTypeAlias"])
                {
                    case "WikiPage":
                        //Wiki uses bodyText field
                        if (result.Fields.ContainsKey("bodyText"))
                            text = result.Fields["bodyText"];
                        break;
                    case "Project":
                        //Project uses description field
                        if (result.Fields.ContainsKey("descripiton"))
                            text = result.Fields["description"];
                        break;
                }

            }
            else if (result["__IndexType"] == "documents")
            {

                try
                {
                    if (result.Fields.ContainsKey("Body"))
                        text = result["Body"];
                }
                catch { }
            }
            else if (result["__IndexType"] == "documentation")
            {

                try
                {
                    if (result.Fields.ContainsKey("Body"))
                        text = result["Body"];
                }
                catch { }
            }

            text = umbraco.library.StripHtml(text);

            if (text.Length > noOfChars && text.Length > 0)
            {
                text = text.Substring(0, noOfChars) + "...";
            }

            return text;

        }

        public static string cssClassName(this SearchResult result)
        {
            string cssClass = string.Empty;

            if (result["__IndexType"] == "content")
            {
                switch (result.Fields["nodeTypeAlias"])
                {
                    case "WikiPage":
                        cssClass = " wiki ";
                        break;
                    case "Project":
                        cssClass = " project ";
                        break;
                }
            }
            else if (result["__IndexType"] == "documentation")
            {
                cssClass = " documentation ";
            }
            else if (result["__IndexType"] == "documents")
            {
                cssClass = " forum ";


                SqlConnection cn = new SqlConnection(umbraco.GlobalSettings.DbDSN);
                cn.Open();
                SqlCommand cmd = new SqlCommand("Select answer from forumTopics where id = @id", cn);
                cmd.Parameters.AddWithValue("@id", result.Id);
                int solved = 0;
                try
                {
                    int.TryParse(cmd.ExecuteScalar().ToString(), out solved);
                }
                catch { }
                cn.Close();

                if (solved != 0)
                    cssClass += "solution";
            }


            return cssClass;

        }

        public static string BuildExamineString(this string term, int boost, string field, bool andSearch)
        {
            term = Lucene.Net.QueryParsers.QueryParser.Escape(term);
            var terms = term.Trim().Split(' ');
            var qs = field + ":";
            qs += "\"" + term + "\"^" + (boost + 30000).ToString() + " ";
            qs += field + ":(+" + term.Replace(" ", " +") + ")^" + (boost + 5).ToString() + " ";
            if (!andSearch)
            {
                qs += field + ":(" + term + ")^" + boost.ToString() + " ";
            } return qs;
        }


    }

    public partial class ExamineSearchResults : System.Web.UI.UserControl
    {

        /// <summary>
        /// wiki,forum,project,documentation
        /// </summary>
        static readonly Dictionary<Tuple<bool, bool, bool, bool>, Func<IEnumerable<SearchResult>, IEnumerable<SearchResult>>> lookup = new Dictionary<Tuple<bool, bool, bool, bool>, Func<IEnumerable<SearchResult>, IEnumerable<SearchResult>>>();

        /// <summary>
        /// What we are searching for...
        /// </summary>
        protected string searchTerm { get; private set; }

        /// <summary>
        /// This is where we will store the search results list
        /// </summary>
        protected IEnumerable<SearchResult> searchResults { get; private set; }


        protected BaseSearchProvider Searcher;



        public ExamineSearchResults()
        {
            searchTerm = string.Empty;
            searchResults = new List<SearchResult>();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitLookUpDictionary();

            //Check we have a search term
            var orgSearchTerm = Request.QueryString["q"].Trim();
            searchTerm = orgSearchTerm.Replace(" OR ", " ").Replace(" or ", " ");
            if (string.IsNullOrEmpty(searchTerm) || searchTerm.Length == 1)
            {
                phNotValid.Visible = true;
                phResults.Visible = false;
                return;
            }


            Searcher = ExamineManager.Instance.SearchProviderCollection["MultiIndexSearcher"];

            //Search Criteria for WIKI & Projects
            bool andSearch = false;
            var searchCriteria = Searcher.CreateSearchCriteria(BooleanOperation.Or);
            if (searchTerm.IndexOf("\"") == -1 && searchTerm.ToLower().IndexOf(" and ") > -1)
            {
                andSearch = true;
                searchTerm = searchTerm.Replace(" and ", " ").Replace(" AND ", " ");
            }

            /*var searchFilter = searchCriteria.Field("a","b").Or().GroupedOr
               .GroupedOr(new string[] { "nodeName", "bodyText", "description", "Title", "Body", "CommentsContent" }, searchTerm)
               .Compile();*/



            var searchQuery = searchTerm.BuildExamineString(10, "nodeName", andSearch);
            searchQuery += searchTerm.BuildExamineString(8, "bodyText", andSearch);
            searchQuery += searchTerm.BuildExamineString(9, "description", andSearch);
            searchQuery += searchTerm.BuildExamineString(10, "Title", andSearch);
            searchQuery += searchTerm.BuildExamineString(8, "Body", andSearch);
            searchQuery += searchTerm.BuildExamineString(7, "CommentsContent", andSearch).TrimEnd(' ');

            var searchFilter = searchCriteria.RawQuery(searchQuery);


            searchResults = Searcher.Search(searchFilter).OrderByDescending(x => x.Score);

            // set the searchterm back for the results view
            searchTerm = orgSearchTerm;

            //Get where to search (content)
            string searchWhere = Request.QueryString["content"];

            searchResults = FilterOnContentType(searchWhere, searchResults);

            BindResultsAndSetUpPaging(searchResults);

        }

        /// <summary>
        /// init lookup dicitonary with all checkbox combinations and map 
        /// the options to a filter method replaces the need for big if else 
        /// </summary>
        private void InitLookUpDictionary()
        {
            if (lookup.Keys.Count == 0)
            {
                /// making assumption for truth table as = wiki,forum,project,documentation
                lookup.Add(Tuple.Create(true, true, true, true), value => All(value));
                lookup.Add(Tuple.Create(false, false, false, false), value => All(value));

                //single selections
                lookup.Add(Tuple.Create(true, false, false, false), value => WikiOnly(value));
                lookup.Add(Tuple.Create(false, true, false, false), value => ForumOnly(value));
                lookup.Add(Tuple.Create(false, false, true, false), value => ProjectsOnly(value));
                lookup.Add(Tuple.Create(false, false, false, true), value => DocumentationOnly(value));

                //doubles
                lookup.Add(Tuple.Create(true, true, false, false), value => WikiAndForum(value));
                lookup.Add(Tuple.Create(false, true, true, false), value => ProjectAndForum(value));
                lookup.Add(Tuple.Create(false, false, true, true), value => ProjectsAndDocumentation(value));
                lookup.Add(Tuple.Create(false, true, false, true), value => ForumAndDocumentation(value));
                lookup.Add(Tuple.Create(true, false, false, true), value => WikiAndDocumentation(value));
                lookup.Add(Tuple.Create(true, false, true, false), value => WikiAndProjects(value));


                //triples
                lookup.Add(Tuple.Create(false, true, true, true), value => AllExceptWiki(value));
                lookup.Add(Tuple.Create(true, false, true, true), value => AllExceptForum(value));
                lookup.Add(Tuple.Create(true, true, false, true), value => AllExceptProjects(value));
                lookup.Add(Tuple.Create(true, true, true, false), value => AllExceptDocumentation(value));
            }

        }



        /// <summary>
        /// further filtering on content type searching for.
        /// ideally this should be done using examine!!!
        /// </summary>
        /// <param name="searchWhere"></param>
        /// <param name="searchResults"></param>
        /// <returns></returns>
        private IEnumerable<SearchResult> FilterOnContentType(string searchWhere, IEnumerable<SearchResult> searchResults)
        {

            //do not change or of tests else lookup will be incorrect

            searchResults = lookup[Tuple.Create(searchWhere.Contains("wiki"),
                                                searchWhere.Contains("forum"),
                                                searchWhere.Contains("project"),
                                                searchWhere.Contains("documentation"))](searchResults);

            return searchResults;
        }

        #region search results filters
        private static IEnumerable<SearchResult> All(IEnumerable<SearchResult> searchResults)
        {
            searchResults = from r in searchResults where r["__IndexType"] == "documentation" || r["__IndexType"] == "documents" || r["nodeTypeAlias"] == "WikiPage" || (r["nodeTypeAlias"] == "Project" && r["projectLive"] == "1") select r;
            return searchResults;
        }

        private static IEnumerable<SearchResult> AllExceptWiki(IEnumerable<SearchResult> searchResults)
        {
            searchResults = from r in searchResults where r["nodeTypeAlias"] != "WikiPage" select r;
            return searchResults;
        }

        private static IEnumerable<SearchResult> AllExceptDocumentation(IEnumerable<SearchResult> searchResults)
        {
            searchResults = from r in searchResults where r["__IndexType"] != "documentation" select r;
            return searchResults;
        }

        private static IEnumerable<SearchResult> AllExceptForum(IEnumerable<SearchResult> searchResults)
        {
            searchResults = from r in searchResults where r["__IndexType"] != "documents" select r;
            return searchResults;
        }

        private static IEnumerable<SearchResult> AllExceptProjects(IEnumerable<SearchResult> searchResults)
        {
            searchResults = from r in searchResults where r["nodeTypeAlias"] != "Project" select r;
            return searchResults;
        }

        private static IEnumerable<SearchResult> ProjectAndForum(IEnumerable<SearchResult> searchResults)
        {
            searchResults = from r in searchResults where r["__IndexType"] == "documents" || (r["nodeTypeAlias"] == "Project" && r["projectLive"] == "1") select r;
            return searchResults;
        }

        private static IEnumerable<SearchResult> ProjectsAndDocumentation(IEnumerable<SearchResult> searchResults)
        {
            searchResults = from r in searchResults where r["nodeTypeAlias"] == "document" || (r["nodeTypeAlias"] == "Project" && r["projectLive"] == "1") select r;
            return searchResults;
        }

        private static IEnumerable<SearchResult> ForumAndDocumentation(IEnumerable<SearchResult> searchResults)
        {
            searchResults = from r in searchResults where r["__IndexType"] == "documents" || r["__IndexType"] == "documentation" select r;
            return searchResults;
        }

        private static IEnumerable<SearchResult> WikiAndDocumentation(IEnumerable<SearchResult> searchResults)
        {
            searchResults = from r in searchResults where r["nodeTypeAlias"] == "WikiPage" || r["__IndexType"] == "documentation" select r;
            return searchResults;
        }

        private static IEnumerable<SearchResult> WikiAndForum(IEnumerable<SearchResult> searchResults)
        {
            searchResults = from r in searchResults where r["__IndexType"] == "documents" || r["nodeTypeAlias"] == "WikiPage" select r;
            return searchResults;
        }

        private static IEnumerable<SearchResult> WikiAndProjects(IEnumerable<SearchResult> searchResults)
        {
            searchResults = from r in searchResults where r["__IndexType"] == "content" && (r["nodeTypeAlias"] == "WikiPage" || (r["nodeTypeAlias"] == "Project" && r["projectLive"] == "1")) select r;
            return searchResults;
        }

        private static IEnumerable<SearchResult> ForumOnly(IEnumerable<SearchResult> searchResults)
        {
            searchResults = from r in searchResults where r["__IndexType"] == "documents" select r;
            return searchResults;
        }

        private static IEnumerable<SearchResult> ProjectsOnly(IEnumerable<SearchResult> searchResults)
        {
            searchResults = from r in searchResults where r["__IndexType"] == "content" && (r["nodeTypeAlias"] == "Project" && r["projectLive"] == "1") select r;
            return searchResults;
        }

        private static IEnumerable<SearchResult> WikiOnly(IEnumerable<SearchResult> searchResults)
        {
            searchResults = from r in searchResults where r["__IndexType"] == "content" && r["nodeTypeAlias"] == "WikiPage" select r;
            return searchResults;
        }

        private static IEnumerable<SearchResult> DocumentationOnly(IEnumerable<SearchResult> searchResults)
        {
            searchResults = from r in searchResults where r["__IndexType"] == "documentation" select r;
            return searchResults;
        }
        #endregion

        private void BindResultsAndSetUpPaging(IEnumerable<SearchResult> searchResults)
        {
            //Setup paging. If there isn't a page specified default to page 0
            int page = 0;
            int.TryParse(Request.QueryString["p"], out page);
            int ItemsPerPage = 20;


            //Bind repater to the list of results
            searchResultListing.DataSource = searchResults.Skip(page * ItemsPerPage).Take(ItemsPerPage);
            searchResultListing.DataBind();


            //Page numbering...
            int numberOfResults = searchResults.Count();
            int numberOfPages = (int)Math.Round((float)numberOfResults / (float)ItemsPerPage, 0);


            if (numberOfPages > 1)
            {
                //Lets generate the HTML string up for the pager....
                pager.Text = "<div><strong>Pages:</strong></div><ul class=\"deliPaging\">";

                for (int i = 0; i < numberOfPages; i++)
                {
                    pager.Text += "<li>";

                    if (page == i)
                        pager.Text += "<a href='?q=" + searchTerm + "&content=" + Request.QueryString["content"] + "&p=" + i + "' class='selected'>";
                    else
                        pager.Text += "<a href='?q=" + searchTerm + "&content=" + Request.QueryString["content"] + "&p=" + i + "'>";

                    pager.Text += i + 1;
                    pager.Text += "</a>";
                    pager.Text += "</li>";
                }

                pager.Text += "</ul>";
            }
        }
    }
}