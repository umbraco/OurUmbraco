using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;
using Examine;
using Newtonsoft.Json;
using OurUmbraco.Forum.Services;
using OurUmbraco.Our.Api;
using Umbraco.Web.Mvc;
using SearchResult = Tweetinvi.Logic.Model.SearchResult;

namespace OurUmbraco.Community.Controllers
{
    public class StatisticsController : SurfaceController
    {
        public readonly string JsonPath = HostingEnvironment.MapPath("~/App_Data/TEMP/ForumStatisticsData.json");

        public ActionResult ForumStatistics(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate == null)
                fromDate = DateTime.Now.Add(TimeSpan.FromDays(-365));
            if (toDate == null)
                toDate = DateTime.MaxValue;

            var groupedTopicData = GetGroupedTopicData((DateTime)fromDate, (DateTime)toDate);

            return PartialView("~/Views/Partials/Community/ForumStatistics.cshtml", groupedTopicData);
        }

        private GroupedTopicData GetGroupedTopicData(DateTime fromDate, DateTime toDate)
        {
            var allTopics = GetAllTopics().Where(x => x.Created >= fromDate && x.Created <= toDate).ToList();

            var groupedTopicData = new GroupedTopicData
            {
                Labels = new List<string>(),
                DataSets = new List<DataSet>()
            };

            var groupedTopics = allTopics.GroupBy(d => new
            {
                d.Created.Year,
                //WeekNumber = currentCulture.Calendar.GetWeekOfYear(d.Created, 
                //currentCulture.DateTimeFormat.CalendarWeekRule, currentCulture.DateTimeFormat.FirstDayOfWeek)
                d.Created.Month
            }).ToList();

            groupedTopicData.DataSets = new List<DataSet>();
            foreach (var topicGroup in groupedTopics)
            {
                groupedTopicData.Labels.Add(string.Format("{0} {1}",
                    CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(topicGroup.Key.Month), topicGroup.Key.Year));
            }

            //
            var topicCountDataSet = new DataSet
            {
                Label = "Topics",
                Fill = false,
                BackgroundColor = "rgb(0, 0, 255, 0.2)",
                BorderColor = "rgb(0, 0, 255)",
                BorderWidth = 2,
                Data = new List<int>()
            };
            foreach (var topicGroup in groupedTopics)
            {
                var topicsInGroup = topicGroup.ToList();
                topicCountDataSet.Data.Add(topicsInGroup.Count);
            }

            groupedTopicData.DataSets.Add(topicCountDataSet);
            //

            //
            var solvedTopicCountDataSet = new DataSet
            {
                Label = "Solved topics",
                Fill = false,
                BackgroundColor = "rgb(0, 255, 0, 0.2)",
                BorderColor = "rgb(0, 255, 0)",
                BorderWidth = 2,
                Data = new List<int>()
            };

            foreach (var topicGroup in groupedTopics)
            {
                var topicsInGroup = topicGroup.ToList();
                solvedTopicCountDataSet.Data.Add(topicsInGroup.Count(x => x.Answer != 0));
            }

            groupedTopicData.DataSets.Add(solvedTopicCountDataSet);
            //

            //
            var unsolvedTopicCountDataSet = new DataSet
            {
                Label = "Unsolved topics",
                Fill = false,
                BackgroundColor = "rgb(255, 0, 128, 0.2)",
                BorderColor = "rgb(255, 0, 128)",
                BorderWidth = 2,
                Data = new List<int>()
            };

            foreach (var topicGroup in groupedTopics)
            {
                var topicsInGroup = topicGroup.ToList();
                unsolvedTopicCountDataSet.Data.Add(topicsInGroup.Count(x => x.Answer == 0));
            }

            groupedTopicData.DataSets.Add(unsolvedTopicCountDataSet);
            //

            //
            var repliesCountDataSet = new DataSet
            {
                Label = "Replies",
                Fill = false,
                BackgroundColor = "rgb(0, 255, 255, 0.2)",
                BorderColor = "rgb(0, 255, 255)",
                BorderWidth = 2,
                Data = new List<int>()
            };

            foreach (var topicGroup in groupedTopics)
            {
                var topicsInGroup = topicGroup.ToList();
                repliesCountDataSet.Data.Add(topicsInGroup.Sum(x => x.Replies));
            }

            groupedTopicData.DataSets.Add(repliesCountDataSet);
            //

            //
            var topicsNoRepliesCountDataSet = new DataSet
            {
                Label = "Topics with no replies",
                Fill = false,
                BackgroundColor = "rgb(255, 0, 0, 0.2)",
                BorderColor = "rgb(255, 0, 0)",
                BorderWidth = 2,
                Data = new List<int>()
            };

            foreach (var topicGroup in groupedTopics)
            {
                var topicsInGroup = topicGroup.ToList();
                topicsNoRepliesCountDataSet.Data.Add(topicsInGroup.Count(x => x.Replies == 0));
            }

            groupedTopicData.DataSets.Add(topicsNoRepliesCountDataSet);
            //

            return groupedTopicData;
        }

        private IEnumerable<MinimalTopic> GetAllTopics(bool refreshAll = false)
        {
            var topics = new List<MinimalTopic>();
            if (System.IO.File.Exists(JsonPath) && refreshAll == false)
            {
                var jsonData = System.IO.File.ReadAllText(JsonPath);
                topics = JsonConvert.DeserializeObject<List<MinimalTopic>>(jsonData);
            }
            else
            {
                var topicService = new TopicService(DatabaseContext);

                //var currentCulture = CultureInfo.CurrentCulture;
                var topicData = topicService.GetAllTopics();
                foreach (var topic in topicData)
                {
                    var minimalTopic = new MinimalTopic
                    {
                        Id = topic.Id,
                        Created = topic.Created,
                        Replies = topic.Replies,
                        Answer = topic.Answer
                    };
                    topics.Add(minimalTopic);
                }

                // Serialize the data to raw JSON
                var rawJson = JsonConvert.SerializeObject(topics, Formatting.Indented);

                // Save the JSON to disk
                System.IO.File.WriteAllText(JsonPath, rawJson, Encoding.UTF8);
            }

            return topics;
        }

        public ActionResult KarmaStatistics()
        {
            var lastweekStart = GetLastWeekStartDate();
            var karmaRecent = Our.Api.StatisticsController.GetPeopleData(lastweekStart, DateTime.Now);
            
            foreach (var period in karmaRecent.MostActiveDateRange)
            {
                foreach (var person in period.MostActive)
                {
                    var criteria = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"].CreateSearchCriteria();
                    var filter = criteria.RawQuery("__NodeId: " + person.MemberId);
                    var searchResult = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"].Search(filter).FirstOrDefault();
                    if (searchResult == null)
                        continue;

                    int totalKarma;
                    if (int.TryParse(searchResult.Fields["reputationTotal"], out totalKarma))
                        person.TotalKarma = totalKarma;
                }
            }

            // Yearly karma counts from May 1st until May 1st each year. If the date in this year is before May 1st, shift back an extra year
            var yearShift = 0;
            if (DateTime.Now > new DateTime(DateTime.Now.Year, 5, 1))
                yearShift = 1;
            var karmaYear = Our.Api.StatisticsController.GetPeopleData(new DateTime(DateTime.Now.Year - 1 - yearShift, 5, 1), new DateTime(DateTime.Now.Year - yearShift, 5, 1));
            
            foreach (var period in karmaYear.MostActiveDateRange)
            {
                foreach (var person in period.MostActive)
                {
                    var criteria = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"].CreateSearchCriteria();
                    var filter = criteria.RawQuery("__NodeId: " + person.MemberId);
                    var searchResult = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"].Search(filter).FirstOrDefault();
                    if (searchResult == null)
                        continue;

                    int totalKarma;
                    if (int.TryParse(searchResult.Fields["reputationTotal"], out totalKarma))
                        person.TotalKarma = totalKarma;
                }
            }

            var karmaStatistics = new KarmaStatistics
            {
                KarmaRecent = karmaRecent,
                KarmaYear = karmaYear
            };


            return PartialView("~/Views/Partials/Community/KarmaStatistics.cshtml", karmaStatistics);
        }

        private static DateTime GetLastWeekStartDate()
        {
            DateTime date = DateTime.Now.AddDays(-7);
            while (date.DayOfWeek != DayOfWeek.Monday)
            {
                date = date.AddDays(-1);
            }

            return date;
        }
    }

    public class MinimalTopic
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public int Replies { get; set; }
        public int Answer { get; set; }
    }

    public class KarmaStatistics
    {
        public PeopleData KarmaRecent { get; set; }
        public PeopleData KarmaYear { get; set; }
    }

    public class GroupedTopicData
    {
        [JsonProperty(PropertyName = "labels")]
        public List<string> Labels { get; set; }

        [JsonProperty(PropertyName = "datasets")]
        public List<DataSet> DataSets { get; set; }
    }

    public class DataSet
    {
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "fill")]
        public bool Fill { get; set; }

        [JsonProperty(PropertyName = "backgroundColor")]
        public string BackgroundColor { get; set; }

        [JsonProperty(PropertyName = "borderWidth")]
        public int BorderWidth { get; set; }

        [JsonProperty(PropertyName = "borderColor")]
        public string BorderColor { get; set; }

        [JsonProperty(PropertyName = "data")]
        public List<int> Data { get; set; }
    }
}
