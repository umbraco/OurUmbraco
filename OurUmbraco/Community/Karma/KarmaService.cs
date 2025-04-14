using System.IO;
using System.Web.Hosting;
using Newtonsoft.Json;
using OurUmbraco.Community.Controllers;

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
    }
}
