using System.Collections.Generic;
using Examine.SearchCriteria;
using OurUmbraco.Forum.Services;
using OurUmbraco.Our.Examine;
using OurUmbraco.Project.Services;
using umbraco;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Api
{
    public class StatisticsController : UmbracoAuthorizedApiController
    {
        [System.Web.Http.HttpGet]
        public TopicData GetTopicData()
        {
            var topicService = new TopicService(DatabaseContext);
            var allSolvedTopics = topicService.GetAllTopicsCount(unsolved: false);
            var allUnsolvedTopics = topicService.GetAllTopicsCount(unsolved: true);

            var topicData = new TopicData
            {
                AllTopics = allSolvedTopics + allUnsolvedTopics,
                AllSolvedTopics = allSolvedTopics,
                AllUnsolvedTopics = allUnsolvedTopics
            };

            return topicData;
        }

        [System.Web.Http.HttpGet]
        public ProjectData GetProjectData()
        {
            var searchFilters = new SearchFilters(BooleanOperation.And);
            //MUST be live
            searchFilters.Filters.Add(new SearchFilter("projectLive", "1"));
            var filters = new List<SearchFilters> { searchFilters };

            var searcher = new OurSearcher(null, "project", filters: filters, maxResults: 20000);
            var results = searcher.Search("projectSearcher");
            var projects = results.SearchResults.TotalItemCount;

            var totalDownloads = 0;
            foreach (var project in results.SearchResults)
            {
                var downloads = project.Fields["downloads"];
                int downloadCount;
                if (int.TryParse(downloads, out downloadCount))
                    totalDownloads = totalDownloads + downloadCount;
            }

            var compatReport = new VersionCompatibilityService(DatabaseContext);
            var compatibilityReportsCount = compatReport.GetAllCompatibilityReportsCount();
            var projectData = new ProjectData
            {
                AllCompatibilityReports =  compatibilityReportsCount,
                AllProjects = projects,
                AllProjectDownloads = totalDownloads
            };

            return projectData;
        }
    }

    public class TopicData
    {
        public int AllTopics { get; set; }
        public int AllSolvedTopics { get; set; }
        public int AllUnsolvedTopics { get; set; }
    }

    public class ProjectData
    {
        public int AllProjects { get; set; }
        public int AllCompatibilityReports { get; set; }
        public int AllProjectDownloads { get; set; }
    }
}
