using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using OurUmbraco.Wiki.BusinessLogic;

namespace OurUmbraco.Our.Models
{
    public class WikiFileModel
    {
        public int ProjectId { get; set; }
        public HttpPostedFileBase File { get; set; }
        public string FileType { get; set; }
        public IList<SelectListItem> AvailableVersions { get; set; }
        public List<string> SelectedVersions { get; set; }
        public string DotNetVersion { get; set; }
        
        public static IList<SelectListItem> GetUmbracoVersions()
        {
            var umbracoVersions = new List<SelectListItem>();
            foreach (var uv in UmbracoVersion.AvailableVersions().Values)
            {
                umbracoVersions.Add(new SelectListItem { Text = uv.Name, Value = uv.Version });
            }

            return umbracoVersions;
        }
    }
}
