using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;
using Newtonsoft.Json;
using OurUmbraco.Forum.Services;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Community.Controllers
{
    public class StatisticsController : SurfaceController
    {
        public readonly string JsonPath = HostingEnvironment.MapPath("~/App_Data/TEMP/ForumStatisticsData.json");

        public ActionResult Statistics()
        {
            GroupedTopicData groupedTopicData;

            if (System.IO.File.Exists(JsonPath))
            {
                var jsonData = System.IO.File.ReadAllText(JsonPath);
                groupedTopicData = JsonConvert.DeserializeObject<GroupedTopicData>(jsonData);
            }
            else
            {
                groupedTopicData = GetGroupedTopicData();
            }

            return PartialView("~/Views/Partials/Home/ForumStatistics.cshtml", groupedTopicData);
        }

        private GroupedTopicData GetGroupedTopicData()
        {
            var topicService = new TopicService(DatabaseContext);

            //var currentCulture = CultureInfo.CurrentCulture;
            var topics = topicService.GetAllTopicsByDateRange(new DateTime(1990, 1, 1), DateTime.MaxValue);
            var allTopics = topics.GroupBy(d => new
            {
                d.Created.Year,
                //WeekNumber = currentCulture.Calendar.GetWeekOfYear(d.Created, 
                //currentCulture.DateTimeFormat.CalendarWeekRule, currentCulture.DateTimeFormat.FirstDayOfWeek)
                d.Created.Month
            }).ToList();

            var groupedTopicData = new GroupedTopicData
            {
                Labels = new List<string>(),
                DataSets = new List<DataSet>()
            };

            groupedTopicData.DataSets = new List<DataSet>();
            foreach (var topicGroup in allTopics)
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
            foreach (var topicGroup in allTopics)
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

            foreach (var topicGroup in allTopics)
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

            foreach (var topicGroup in allTopics)
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

            foreach (var topicGroup in allTopics)
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

            foreach (var topicGroup in allTopics)
            {
                var topicsInGroup = topicGroup.ToList();
                topicsNoRepliesCountDataSet.Data.Add(topicsInGroup.Count(x => x.Replies == 0));
            }

            groupedTopicData.DataSets.Add(topicsNoRepliesCountDataSet);
            //

            // Serialize the data to raw JSON
            var rawJson = JsonConvert.SerializeObject(groupedTopicData, Formatting.Indented);

            // Save the JSON to disk
            System.IO.File.WriteAllText(JsonPath, rawJson, Encoding.UTF8);
            return groupedTopicData;
        }
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
