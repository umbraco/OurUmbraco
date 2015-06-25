using System;
using System.Collections.Generic;
using Examine.SearchCriteria;
using InfoCaster.Umbraco.UrlTracker.Helpers;
using our.Examine;
using our.Models;
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
                var umbracoPage = UmbracoContext.PublishedContentRequest.PublishedContent;

                var nodeTypeAlias = Request.QueryString["cat"];
                
                int forumId;
                var forumName = string.Empty;
                var filters = new List<SearchFilters>();
                if (nodeTypeAlias == "forum" && int.TryParse(Request.QueryString["fid"], out forumId))
                {
                    var searchFilters = new SearchFilters(BooleanOperation.And);
                    searchFilters.Filters.Add(new SearchFilter("parentId", forumId.ToString()));
                    filters.Add(searchFilters);

                    var umbracoHelper = new Umbraco.Web.UmbracoHelper(Umbraco.Web.UmbracoContext.Current);
                    var forum = umbracoHelper.ContentQuery.TypedContent(forumId);
                    var parentForum = forum.Parent;
                    forumName = forum.Name + " - " + parentForum.Name;
                }

                var ourSearcher = new OurSearcher(Request.QueryString["q"], 
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