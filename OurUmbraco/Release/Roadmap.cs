using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using OurUmbraco.Release.Models;

namespace OurUmbraco.Release
{
    public static class Roadmap
    {
        public static IEnumerable<AggregateView> GetRoadmapReleasesFromFile()
        {
            var import = new Import();
            if (File.Exists(HttpContext.Current.Server.MapPath(import.YouTrackJsonFile)) == false)
                import.SaveAllToFile();

            var allText = File.ReadAllText(HttpContext.Current.Server.MapPath(import.YouTrackJsonFile));

            var data = new JavaScriptSerializer().Deserialize<List<AggregateView>>(allText);
            var result = data.Where(x => x.released == false && x.isPatch == false);

            return result;
        }
    }
}
