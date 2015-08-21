using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using System.Web.Script.Serialization;
using OurUmbraco.Release.Models;

namespace OurUmbraco.Release
{
    public class Versions
    {
        public List<AggregateView> GetAggregateVersionsFromFile()
        {
            var import = new Import();
            var allText = File.ReadAllText(HostingEnvironment.MapPath(import.YouTrackJsonFile));

            return new JavaScriptSerializer().Deserialize<List<AggregateView>>(allText);
        }
    }
}
