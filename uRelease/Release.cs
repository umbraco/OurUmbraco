using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using uRelease.Controllers;
using uRelease.Models;

namespace uRelease
{
    public class Release
    {
        public static List<AggregateView> GetAllReleaseFromFile()
        {
            var releaseController = new ReleaseController();
            if (File.Exists(HttpContext.Current.Server.MapPath(ReleaseController.YouTrackJsonFile)) == false)
                releaseController.SaveAllToFile();

            var allText = File.ReadAllText(HttpContext.Current.Server.MapPath(ReleaseController.YouTrackJsonFile));

            var data = new JavaScriptSerializer().Deserialize<List<AggregateView>>(allText);

            return data;
        }
    }
}
