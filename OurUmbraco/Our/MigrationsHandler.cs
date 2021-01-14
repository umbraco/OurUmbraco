using System.IO;
using System.Net;
using System.Web.Hosting;
using Newtonsoft.Json;
using Umbraco.Core;

namespace OurUmbraco.Our
{
    public class MigrationsHandler : ApplicationEventHandler
    {
        private const string MigrationMarkersPath = "~/App_Data/migrations/";

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication,
            ApplicationContext applicationContext)
        {
            EnsureMigrationsMarkerPathExists();
            GetReleasesJson();
        }

        private void EnsureMigrationsMarkerPathExists()
        {
            var path = HostingEnvironment.MapPath(MigrationMarkersPath);
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);
        }

        private void GetReleasesJson()
        {
            var releasesCacheFile = HostingEnvironment.MapPath("~/App_Data/TEMP/Releases.json");
            if (File.Exists(releasesCacheFile) == false)
            {
                using (var client = new WebClient())
                {

                    var json = client.DownloadString("https://our.umbraco.com/webapi/releases/GetReleasesCache");
                    var rawString = JsonConvert.DeserializeObject<string>(json);
                    File.WriteAllText(releasesCacheFile, rawString);

                }
            }
        }
    }
}