using System;
using System.Web.Mvc;
using uProject.Models;
using uProject.Services;
using uProject.uVersion;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;

namespace uProject.Controllers
{
    public class ProjectCompatibilityReportController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult CompatibilityReport(int projectId, int fileId)
        {
            var compatReport = new VersionCompatibilityService(DatabaseContext);

            var currentMember = Members.IsLoggedIn() ? Members.GetCurrentMember() : null;

            return PartialView("~/Views/Partials/Projects/CompatibilityReport.cshtml", 
                new VersionCompatibilityReportModel
                {
                    VersionCompatibilities = compatReport.GetCompatibilityReport(projectId),
                    CurrentMemberHasDownloaded = currentMember != null && uProject.library.HasDownloaded(currentMember.Id, projectId),
                    CurrentMemberIsLoggedIn = currentMember != null,
                    FileId = fileId,
                    ProjectId = projectId,
                    AllVersions = UVersion.GetAllVersions()
                });
        }

       
    }
}