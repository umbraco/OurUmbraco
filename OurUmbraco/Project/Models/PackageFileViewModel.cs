using OurUmbraco.Wiki.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.ModelBinding;

namespace OurUmbraco.Project.Models
{
    public class PackageFileViewModel
    {
        public string FileType { get; set; }
        public List<UmbracoVersion> UmbracoVersions { get; set; }
        public bool IsCurrent { get; set; }
        public string DotNetVersion { get; set; }
        public HttpPostedFile Package{ get; set; }
    }


}
