using System.Linq;
using System.Web.Mvc;
using OurUmbraco.Our;
using OurUmbraco.Project.Models;
using OurUmbraco.Project.Services;
using OurUmbraco.Project.uVersion;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Project.Controllers
{
    public class ProjectCompatibilityReportController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult CompatibilityReport(int projectId, int fileId)
        {
            var compatReport = new VersionCompatibilityService(DatabaseContext);

            var currentMember = Members.IsLoggedIn() ? Members.GetCurrentMember() : null;

            var project = Umbraco.TypedContent(projectId);

            var versions = new UVersion();
            
            return PartialView("~/Views/Partials/Projects/CompatibilityReport.cshtml", 
                new VersionCompatibilityReportModel
                {
                    VersionCompatibilities = compatReport.GetCompatibilityReport(projectId),
                    CurrentMemberIsLoggedIn = currentMember != null,
                    FileId = fileId,
                    ProjectId = projectId,
                    AllVersions = versions.GetAllVersions(),
                    WorksOnUaaS = project.GetPropertyValue<bool>("worksOnUaaS")
                });
        }

       
    }
}