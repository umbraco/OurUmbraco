using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using OurUmbraco.Wiki.BusinessLogic;

namespace OurUmbraco.Our.Models
{
    public class WikiFileModel
    {
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public HttpPostedFileBase File { get; set; }
        [Required]
        public string FileType { get; set; }
        public IList<SelectListItem> AvailableVersions { get; set; }
        [Required]
        [Display(Name = "Supported Umbraco version(s)")]
        public List<string> SelectedVersions { get; set; }
        [Required]
        [Display(Name = "Supported .NET runtime")]
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
