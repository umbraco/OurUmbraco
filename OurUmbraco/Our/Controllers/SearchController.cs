﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Examine.SearchCriteria;
using OurUmbraco.Our.Examine;
using OurUmbraco.Our.Extensions;
using OurUmbraco.Our.Models;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class SearchController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult Render(string q, string cat = "", string order = "", int fid = 0, bool solved = false, bool replies = false, int? v = null)
        {
            // If no search string specified, use a blank one to prevent null exceptions.
            if (string.IsNullOrEmpty(q))
                q = string.Empty;

            if (q.Length > 500)
            {
                q = q.Substring(0, 500);
            }
            else if (q.StartsWith("duplicate content") && q.Contains("SELECT"))
            {
                q = string.Empty;
            } 
            // A particular SQL injection attack uses this query which takes very long to process, turning it into and easy DOS attack
            // /search?q=999999.9' /**/uNiOn/**/aLl /**/sElEcT 0x393133353134353632312e39,0x393133353134353632322e39,0x393133353134353632332e39,0x393133353134353632342e39,0x393133353134353632352e39,0x393133353134353632362e39,0x393133353134353632372e39,0x393133353134353632382e39,0x393133353134353632392e39,0x39313335313435363231302e39,0x39313335313435363231312e39,0x39313335313435363231322e39,0x39313335313435363231332e39,0x39313335313435363231342e39,0x39313335313435363231352e39 and '0'='0--
            else if (q.Contains("999999.9'") || q.Contains("0x393133353134353632392e39"))
            {
                q = string.Empty;
            } 
            else 
            {
                // If first 10 characters contain Chinese, drop the query
                // This in response to a new attack that brings down the server
                var numberOfCharacters = 10;
                if(q.Length < 10) 
                {
                    numberOfCharacters = q.Length;
                }
                var firstCharacters = q.Substring(0, numberOfCharacters);
                if (firstCharacters.Any(character => character.IsChinese()))
                    q = string.Empty;
            }
            
            var umbracoPage = this.CurrentPage;

            var nodeTypeAlias = cat;
            
            var forumName = string.Empty;
            var filters = new List<SearchFilters>();

            if (nodeTypeAlias == "project")
            {
                var searchFilters = new SearchFilters(BooleanOperation.And);
                searchFilters.Filters.Add(new SearchFilter("projectLive", "1"));
                filters.Add(searchFilters);
            }

            if (nodeTypeAlias == "forum" && fid > 0)
            {
                var searchFilters = new SearchFilters(BooleanOperation.And);
                searchFilters.Filters.Add(new SearchFilter("parentId", fid));
                filters.Add(searchFilters);

                var forum = Umbraco.ContentQuery.TypedContent(fid);
                var parentForum = forum.Parent;
                forumName = forum.Name + " - " + parentForum.Name;
            }

            if (solved)
            {
                var searchFilters = new SearchFilters(BooleanOperation.Not);
                searchFilters.Filters.Add(new SearchFilter("solved", "0"));
                filters.Add(searchFilters);
            }

            if (replies)
            {
                var searchFilters = new SearchFilters(BooleanOperation.Not);
                searchFilters.Filters.Add(new SearchFilter("replies", "0"));
                filters.Add(searchFilters);
            }
            var ourSearcher = new OurSearcher(q,
                //TODO: Depending on what order by this is, we need to pass in a data
                // type here, for example, if its an INT or a Date!
                orderBy: order,
                maxResults: 100,
                nodeTypeAlias: nodeTypeAlias,
                majorDocsVersion: v,
                filters: filters);

            var results = ourSearcher.Search();

            var model = new SearchResultContentModel(umbracoPage, results);
            if (string.IsNullOrWhiteSpace(forumName) == false)
                model.Results.Category = forumName;

            return PartialView("~/Views/Partials/SearchResults.cshtml", model);
        }
    }
}
