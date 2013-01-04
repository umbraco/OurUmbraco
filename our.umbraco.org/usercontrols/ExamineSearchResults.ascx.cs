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

            else if (result["__IndexType"] == "documentation") {
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

            if (searchWhere.Contains("documentation") && !searchWhere.Contains("project") && !searchWhere.Contains("forum") && !searchWhere.Contains("wiki")) { 
                //documenation only
                searchResults = from r in searchResults where r["__IndexType"] == "documentation"  select r;
            }
            else if (searchWhere.Contains("wiki") && !searchWhere.Contains("project") && !searchWhere.Contains("forum"))
            {
                //only wiki
                searchResults = from r in searchResults where r["__IndexType"] == "content" && r["nodeTypeAlias"] == "WikiPage" select r;
            }
            else if (!searchWhere.Contains("wiki") && searchWhere.Contains("project") && !searchWhere.Contains("forum"))
            {
                //only projects
                searchResults = from r in searchResults where r["__IndexType"] == "content" && (r["nodeTypeAlias"] == "Project" && r["projectLive"] == "1") select r;
            }
            else if (!searchWhere.Contains("wiki") && !searchWhere.Contains("project") && searchWhere.Contains("forum"))
            {
                //only forum
                searchResults = from r in searchResults where r["__IndexType"] == "documents" select r;
            }
            else if (searchWhere.Contains("wiki") && searchWhere.Contains("project") && !searchWhere.Contains("forum"))
            {
                //wiki and projects
                searchResults = from r in searchResults where r["__IndexType"] == "content" && (r["nodeTypeAlias"] == "WikiPage" || (r["nodeTypeAlias"] == "Project" && r["projectLive"] == "1")) select r;
            }
            else if (searchWhere.Contains("wiki") && !searchWhere.Contains("project") && searchWhere.Contains("forum"))
            {
                //wiki and forum
                searchResults = from r in searchResults where r["__IndexType"] == "documents" || r["nodeTypeAlias"] == "WikiPage" select r;
            }
            else if (!searchWhere.Contains("wiki") && searchWhere.Contains("project") && searchWhere.Contains("forum"))
            {
                //project and forum
                searchResults = from r in searchResults where r["__IndexType"] == "documents" || (r["nodeTypeAlias"] == "Project" && r["projectLive"] == "1") select r;
            }
            else
            {
                searchResults = from r in searchResults where r["__IndexType"] == "documents" || r["nodeTypeAlias"] == "WikiPage" || (r["nodeTypeAlias"] == "Project" && r["projectLive"] == "1") select r;
            }

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