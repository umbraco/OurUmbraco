using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using OurUmbraco.Release.Models;

namespace OurUmbraco.Release
{
    public class Release
    {
        public static List<AggregateView> GetAllReleaseFromFile()
        {
            var import = new Import();
            if (File.Exists(HttpContext.Current.Server.MapPath(import.YouTrackJsonFile)) == false)
                import.SaveAllToFile();

            var allText = File.ReadAllText(HttpContext.Current.Server.MapPath(import.YouTrackJsonFile));

            var data = new JavaScriptSerializer().Deserialize<List<AggregateView>>(allText);

            return data;
        }
    }
}
