using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using Examine;
using Newtonsoft.Json;
using OurUmbraco.Community.Controllers;
using OurUmbraco.Community.People;
using OurUmbraco.NotificationsCore;

namespace OurUmbraco.Community.Karma
{
    public class KarmaService
    {

        private readonly string _karmaStatisticsCacheFile = HostingEnvironment.MapPath("~/App_Data/TEMP/KarmaStatisticsCache.json");

        public KarmaStatistics GetCachedKarmaStatistics()
        {
            var karmaStatisticsCache = new KarmaStatistics();

            if (File.Exists(_karmaStatisticsCacheFile) == false)
                return karmaStatisticsCache;

            var json = File.ReadAllText(_karmaStatisticsCacheFile);
            using (var stringReader = new StringReader(json))
            using (var jsonTextReader = new JsonTextReader(stringReader))
            {
                var jsonSerializer = new JsonSerializer();
                karmaStatisticsCache = jsonSerializer.Deserialize<KarmaStatistics>(jsonTextReader);
            }

            return karmaStatisticsCache;
        }

        public void RefreshKarmaStatistics()
        {
            using (ContextHelper.EnsureHttpContext())
            {
                RefreshKarmaStatisticsInternally();
            }
        }

        private void RefreshKarmaStatisticsInternally()
        {
            var avatarService = new AvatarService();

            var lastweekStart = GetLastWeekStartDate();
            var karmaRecentStatistics = Our.Api.StatisticsController.GetPeopleData(lastweekStart, DateTime.Now);

            foreach (var period in karmaRecentStatistics.MostActiveDateRange)
            {
                foreach (var person in period.MostActive)
                {
                    var criteria = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"].CreateSearchCriteria();
                    var filter = criteria.RawQuery("__NodeId: " + person.MemberId);
                    var searchResult = ExamineManager.Instance
                        .SearchProviderCollection["InternalMemberSearcher"].Search(filter).FirstOrDefault();
                    if (searchResult == null)
                        continue;
                    
                    if (int.TryParse(searchResult.Fields["reputationTotal"], out var totalKarma))
                        person.TotalKarma = totalKarma;
                    
                    person.MemberAvatarUrl = avatarService.GetMemberAvatar(searchResult);
                }
            }

            // Yearly karma counts from May 1st until May 1st each year. If the date in this year is before May 1st, shift back an extra year
            var yearShift = 1;
            if (DateTime.Now >= new DateTime(DateTime.Now.Year, 5, 1))
                yearShift = 0;


            var karmaLastYearStatistics = Our.Api.StatisticsController.GetPeopleData(
                new DateTime(DateTime.Now.Year - 1 - yearShift, 5, 1),
                new DateTime(DateTime.Now.Year - yearShift, 5, 1));

            foreach (var period in karmaLastYearStatistics.MostActiveDateRange)
            {
                foreach (var person in period.MostActive)
                {
                    var criteria = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"].CreateSearchCriteria();
                    var filter = criteria.RawQuery("__NodeId: " + person.MemberId);
                    var searchResult = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"].Search(filter).FirstOrDefault();
                    if (searchResult == null)
                        continue;

                    if (int.TryParse(searchResult.Fields["reputationTotal"], out var totalKarma))
                        person.TotalKarma = totalKarma;
                    
                    person.MemberAvatarUrl = avatarService.GetMemberAvatar(searchResult);
                }
            }

            var karmaThisYearStatistics = Our.Api.StatisticsController.GetPeopleData(
                new DateTime(DateTime.Now.Year - 1, 5, 1),
                new DateTime(DateTime.Now.Year, 5, 1));

            foreach (var period in karmaThisYearStatistics.MostActiveDateRange)
            {
                foreach (var person in period.MostActive)
                {
                    var criteria = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"].CreateSearchCriteria();
                    var filter = criteria.RawQuery("__NodeId: " + person.MemberId);
                    var searchResult = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"]
                        .Search(filter).FirstOrDefault();
                    if (searchResult == null)
                        continue;

                    if (int.TryParse(searchResult.Fields["reputationTotal"], out var totalKarma))
                        person.TotalKarma = totalKarma;
                    
                    person.MemberAvatarUrl = avatarService.GetMemberAvatar(searchResult);
                }
            }

            var karmaStatistics = new KarmaStatistics
            {
                KarmaRecent = karmaRecentStatistics,
                KarmaLastYear = karmaLastYearStatistics,
                KarmaThisYear = karmaThisYearStatistics
            };

            var rawJson = JsonConvert.SerializeObject(karmaStatistics, Formatting.Indented);
            File.WriteAllText(_karmaStatisticsCacheFile, rawJson, Encoding.UTF8);
        }


        private static DateTime GetLastWeekStartDate()
        {
            var date = DateTime.Now.AddDays(-7);
            while (date.DayOfWeek != DayOfWeek.Monday)
                date = date.AddDays(-1);

            return date;
        }
    }
}
