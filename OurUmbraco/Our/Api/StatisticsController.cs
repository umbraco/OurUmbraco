using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Examine.SearchCriteria;
using OurUmbraco.Community.People;
using OurUmbraco.Community.People.Models;
using OurUmbraco.Forum.Services;
using OurUmbraco.Our.Examine;
using OurUmbraco.Project.Services;
using Skybrud.Essentials.Time.Extensions;
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
                AllSolvedTopicsDateRange = limitedSolvedTopics,
                AllUnsolvedTopicsDateRange = limitedUnsolvedTopics
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
        public static PeopleData GetPeopleData(DateTime fromDate, DateTime toDate)
        {
            var peopleData = new PeopleData {  MostActiveDateRange =  new List<PeopleDataByWeek>() };
            var yearAndWeekNumbers = GetYearAndWeekNumbers(fromDate, toDate);
            
            foreach (var yearAndWeekNumber in yearAndWeekNumbers)
            {
                var weekStartDate = GetDateRangeFromWeekNumber(yearAndWeekNumber.Year, yearAndWeekNumber.WeekNumber);
                var weekEndDate = weekStartDate.AddDays(7);

                var peopleService = new PeopleService();
                var topPeople = peopleService.GetMostActiveDateRange(weekStartDate, weekEndDate);

                var peopleDataByWeek = new PeopleDataByWeek
                {
                    WeekNumber = yearAndWeekNumber.WeekNumber,
                    Year = yearAndWeekNumber.Year,
                    MostActive = topPeople
                };
                
                peopleData.MostActiveDateRange.Add(peopleDataByWeek);
            }
            return peopleData;
        }

        // Adapted from: https://stackoverflow.com/a/25248044/5018
        public static List<YearAndWeekNumbers> GetYearAndWeekNumbers(DateTime fromDate, DateTime toDate)
        {
            var currentCulture = CultureInfo.GetCultureInfo("da-DK");
            var yearAndWeekNumbers = new List<YearAndWeekNumbers>();

            for (var dateTime = fromDate; dateTime <= toDate; dateTime = dateTime.AddDays(1))
            {
                var weeekNumber = currentCulture.Calendar.GetWeekOfYear(
                    dateTime,
                    currentCulture.DateTimeFormat.CalendarWeekRule,
                    currentCulture.DateTimeFormat.FirstDayOfWeek);

                var yearAndWeekNumber = new YearAndWeekNumbers { WeekNumber = weeekNumber, Year = dateTime.GetFirstDayOfWeek(DayOfWeek.Monday).Year };
                if (yearAndWeekNumbers.Any(x => x.Year == yearAndWeekNumber.Year && x.WeekNumber == yearAndWeekNumber.WeekNumber) == false)
                    yearAndWeekNumbers.Add(yearAndWeekNumber);
            }

            return yearAndWeekNumbers;
        }

        // Adapted from: https://stackoverflow.com/a/9064954/5018
        public static DateTime GetDateRangeFromWeekNumber(int year, int weekOfYear)
        {
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            var firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.GetCultureInfo("da-DK").Calendar;
            var firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
                weekNum -= 1;

            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
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

    public class PeopleData
    {
        public List<PeopleDataByWeek> MostActiveDateRange { get; set; }
    }

    public class PeopleDataByWeek
    {
        public int WeekNumber { get; set; }
        public int Year { get; set; }
        public List<PeopleKarmaResult> MostActive { get; set; }
    }

    public class YearAndWeekNumbers
    {
        public int Year { get; set; }
        public int WeekNumber { get; set; }
    }
}
