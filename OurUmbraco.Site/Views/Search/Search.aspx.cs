using System;
using System.Collections.Generic;
using Examine.SearchCriteria;
using OurUmbraco.Our.Examine;
using OurUmbraco.Our.Models;
using Umbraco.Web;
using Umbraco.Web.UI.Pages;

namespace OurUmbraco.Site.Views.Search
{
    public partial class Search : BasePage
    {
        protected SearchResultContentModel Model { get; private set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!IsPostBack)
            {
                // A particular SQL injection attack uses this query which takes very long to process, turning it into and easy DOS attack
                // /search?q=999999.9' /**/uNiOn/**/aLl /**/sElEcT 0x393133353134353632312e39,0x393133353134353632322e39,0x393133353134353632332e39,0x393133353134353632342e39,0x393133353134353632352e39,0x393133353134353632362e39,0x393133353134353632372e39,0x393133353134353632382e39,0x393133353134353632392e39,0x39313335313435363231302e39,0x39313335313435363231312e39,0x39313335313435363231322e39,0x39313335313435363231332e39,0x39313335313435363231342e39,0x39313335313435363231352e39 and '0'='0-- 
                var searchQuery = Request.QueryString["q"];
                if(searchQuery.Contains("0x393133353134353632392e39"))
                    Response.Redirect("/search?q=");

                var umbracoPage = UmbracoContext.PublishedContentRequest.PublishedContent;

                var nodeTypeAlias = Request.QueryString["cat"];

                //TODO: If we are searching on projects, they need to be filtered to approved/live!

                int forumId;
                var forumName = string.Empty;
                var filters = new List<SearchFilters>();
                if (nodeTypeAlias == "forum" && int.TryParse(Request.QueryString["fid"], out forumId))
                {
                    var searchFilters = new SearchFilters(BooleanOperation.And);
                    searchFilters.Filters.Add(new SearchFilter("parentId", forumId.ToString()));
                    filters.Add(searchFilters);

                    var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                    var forum = umbracoHelper.ContentQuery.TypedContent(forumId);
                    var parentForum = forum.Parent;
                    forumName = forum.Name + " - " + parentForum.Name;
                }

                if (string.IsNullOrWhiteSpace(Request.QueryString["solved"]) == false)
                {
                    bool onlySolvedItems;
                    if (bool.TryParse(Request.QueryString["solved"], out onlySolvedItems) && onlySolvedItems)
                    {
                        var searchFilters = new SearchFilters(BooleanOperation.Not);
                        searchFilters.Filters.Add(new SearchFilter("solved", "0"));
                        filters.Add(searchFilters);
                    }
                }

                if (string.IsNullOrWhiteSpace(Request.QueryString["replies"]) == false)
                {
                    bool onlyIfReplies;
                    if (bool.TryParse(Request.QueryString["replies"], out onlyIfReplies) && onlyIfReplies)
                    {
                        var searchFilters = new SearchFilters(BooleanOperation.Not);
                        searchFilters.Filters.Add(new SearchFilter("replies", "0"));
                        filters.Add(searchFilters);
                    }
                }

                var orderBy = string.Empty;
                if (string.IsNullOrWhiteSpace(Request.QueryString["order"]) == false)
                {
                    orderBy = Request.QueryString["order"];
                }

                var ourSearcher = new OurSearcher(searchQuery,
                    //TODO: Depending on what order by this is, we need to pass in a data
                    // type here, for example, if its an INT or a Date!
                    orderBy: orderBy,
                    maxResults: 100,
                    nodeTypeAlias:
                    nodeTypeAlias,
                    filters: filters);

                var results = ourSearcher.Search();

                Model = new SearchResultContentModel(umbracoPage, results);
                if (string.IsNullOrWhiteSpace(forumName) == false)
                    Model.Results.Category = forumName;

                SearchText.Text = Model.Results.SearchTerm;
            }
            else
            {
                Response.Redirect("/search?q=" + SearchText.Text);
            }

        }
    }
}