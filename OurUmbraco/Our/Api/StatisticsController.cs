using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using Examine.SearchCriteria;
using Newtonsoft.Json;
using OurUmbraco.Community.GitHub.Models.Cached;
using OurUmbraco.Community.Meetup.Models;
using OurUmbraco.Community.Models;
using OurUmbraco.Forum.Services;
using OurUmbraco.Our.Examine;
using OurUmbraco.Project.Services;
using Skybrud.Essentials.Json;
using Skybrud.Social.Http;
using Skybrud.Social.Meetup;
using Skybrud.Social.Meetup.Models.Events;
using Skybrud.Social.Meetup.Responses.Events;
using Umbraco.Core;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Api
{
    public class StatisticsController : UmbracoAuthorizedApiController
    {
        [System.Web.Http.HttpGet]
        public TopicData GetTopicData(DateTime fromDate, DateTime toDate)
        {
            var topicService = new TopicService(DatabaseContext);
            var allSolvedTopics = topicService.GetAllTopicsCount(unsolved: false);
            var allUnsolvedTopics = topicService.GetAllTopicsCount(unsolved: true);
            var limitedSolvedTopics = topicService.GetAllTopicsCountByDateRange(fromDate, toDate, unsolved: false);
            var limitedUnsolvedTopics = topicService.GetAllTopicsCountByDateRange(fromDate, toDate, unsolved: true);

            var topicData = new TopicData
            {
                AllTopics = allSolvedTopics + allUnsolvedTopics,
                AllSolvedTopics = allSolvedTopics,
                AllUnsolvedTopics = allUnsolvedTopics,
                AllTopicsDateRange = limitedSolvedTopics + limitedUnsolvedTopics,
                AllSolvedTopicsDateRange = allSolvedTopics,
                AllUnsolvedTopicsDateRange = allUnsolvedTopics
            };

            return topicData;
        }

        [System.Web.Http.HttpGet]
        public ProjectData GetProjectData(DateTime fromDate, DateTime toDate)
        {
            var searchFilters = new SearchFilters(BooleanOperation.And);
            //MUST be live
            searchFilters.Filters.Add(new SearchFilter("projectLive", "1"));
            var filters = new List<SearchFilters> { searchFilters };

            var searcher = new OurSearcher(null, "project", filters: filters, maxResults: 20000);
            var results = searcher.Search("projectSearcher");
            var projects = results.SearchResults.TotalItemCount;
            var projectsUpdatedDateRange = 0;
            var projectsCreatedDateRange = 0;
            var totalDownloads = 0;
            foreach (var project in results.SearchResults)
            {
                var downloads = project.Fields["downloads"];
                int downloadCount;
                if (int.TryParse(downloads, out downloadCount))
                    totalDownloads = totalDownloads + downloadCount;

                var updated = project.Fields["updateDate"].Substring(0, 8);
                int updatedDate;
                if (int.TryParse(updated, out updatedDate) && updatedDate > int.Parse(fromDate.ToString("yyyyMMdd")) && updatedDate < int.Parse(toDate.ToString("yyyyMMdd")))
                    projectsUpdatedDateRange++;

                var created = project.Fields["createDate"].Substring(0, 8);
                int createdDate;
                if (int.TryParse(created, out createdDate) && createdDate > int.Parse(fromDate.ToString("yyyyMMdd")) && createdDate < int.Parse(toDate.ToString("yyyyMMdd")))
                    projectsCreatedDateRange++;
            }

            var compatReport = new VersionCompatibilityService(DatabaseContext);
            var compatibilityReportsCount = compatReport.GetAllCompatibilityReportsCount();
            var compatibilityReportsCountPastYear = compatReport.GetAllCompatibilityReportsCountByDateRange(fromDate, toDate);
            var projectData = new ProjectData
            {
                AllCompatibilityReports = compatibilityReportsCount,
                AllProjects = projects,
                AllProjectDownloads = totalDownloads,
                AllCompatibilityReportsDateRange = compatibilityReportsCountPastYear,
                AllProjectsUpdatedDateRange = projectsUpdatedDateRange,
                AllProjectsCreatedDateRange = projectsCreatedDateRange
            };

            return projectData;
        }


        [System.Web.Http.HttpGet]
        public MeetupData GetMeetupData(DateTime fromDate, DateTime toDate)
        {
            var meetupData = new MeetupData();
            
            var meetupCache = new List<MeetupCacheItem>();
            var meetupCacheFile = HttpContext.Current.Server.MapPath("~/App_Data/TEMP/MeetupStatisticsCache.json");
            if (File.Exists(meetupCacheFile))
            {
                var json = File.ReadAllText(meetupCacheFile);
                using (var stringReader = new StringReader(json))
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    var jsonSerializer = new JsonSerializer();
                    meetupCache = jsonSerializer.Deserialize<List<MeetupCacheItem>>(jsonTextReader);
                }
            }

            var eventsDateRange = meetupCache.Where(x => x.Time > fromDate && x.Time < toDate);
            meetupData.MeetupEventsDateRange = eventsDateRange.Count();


            string configPath = HttpContext.Current.Server.MapPath("~/config/MeetupUmbracoGroups.txt");
            // Get the alias (urlname) of each group from the config file
            string[] aliases = File.ReadAllLines(configPath).Where(x => x.Trim() != "").Distinct().ToArray();
            meetupData.AllMeetupGroups = aliases.Length;

            return meetupData;
        }
    }

    public class TopicData
    {
        public int AllTopics { get; set; }
        public int AllSolvedTopics { get; set; }
        public int AllUnsolvedTopics { get; set; }
        public int AllTopicsDateRange { get; set; }
        public int AllSolvedTopicsDateRange { get; set; }
        public int AllUnsolvedTopicsDateRange { get; set; }
    }

    public class ProjectData
    {
        public int AllProjects { get; set; }
        public int AllCompatibilityReports { get; set; }
        public int AllProjectDownloads { get; set; }
        public int AllCompatibilityReportsDateRange { get; set; }
        public int AllProjectsUpdatedDateRange { get; set; }
        public int AllProjectsCreatedDateRange { get; set; }
    }

    public class MeetupData
    {
        public int AllMeetupGroups { get; set; }
        public int MeetupEventsDateRange { get; set; }
    }
}
