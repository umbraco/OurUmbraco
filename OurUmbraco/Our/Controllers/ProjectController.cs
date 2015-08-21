using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using OurUmbraco.MarketPlace.Interfaces;
using OurUmbraco.MarketPlace.ListingItem;
using OurUmbraco.MarketPlace.NodeListing;
using OurUmbraco.Our.Models;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class ProjectController : SurfaceController
    {
        [ChildActionOnly]
        public ActionResult Index(int projectId = 0)
        {
            var project = new ListingItem(null, null);
            if (projectId != 0)
            {
                var nodeListingProvider = new NodeListingProvider();
                project = (ListingItem) nodeListingProvider.GetListing(projectId);

                var memberId = Members.GetCurrentMemberId();
                if ((project.VendorId == memberId) == false && Utils.IsProjectContributor(memberId, projectId) == false)
                {
                    Response.Redirect("/member/profile/projects/", true);
                }
            }

            var model = new ProjectDetails();
            var currentPage = Umbraco.TypedContent(UmbracoContext.PageId.Value);
            var rootNode = currentPage.AncestorOrSelf(1);
            var projects = rootNode.Children(x => x.ContentType.Alias == "Projects").First();
            var categories = projects.Children(x => x.ContentType.Alias == "ProjectGroup" && x.GetPropertyValue<bool>("hqOnly") == false);
            model.ProjectCategories = new List<SelectListItem> { new SelectListItem { Text = string.Empty, Value = string.Empty } };
            foreach (var category in categories)
                model.ProjectCategories.Add(new SelectListItem { Text = category.Name, Value = category.Id.ToString(), Selected = project.CategoryId == category.Id });

            model.License = string.IsNullOrWhiteSpace(project.LicenseName) ? "MIT" : project.LicenseName;
            model.LicenseUrl = string.IsNullOrWhiteSpace(project.LicenseUrl) ? "http://www.opensource.org/licenses/mit-license.php" : project.LicenseUrl;

            model.Title = project.Name;
            model.Description = project.Description;
            model.Version = project.CurrentVersion;
            model.SourceCodeUrl = project.SourceCodeUrl;
            model.DemonstrationUrl = project.DemonstrationUrl;
            model.OpenForCollaboration = project.OpenForCollab;
            model.GoogleAnalyticsCode = project.GACode;
            model.Id = projectId;

            return PartialView("~/Views/Partials/Project/Edit.cshtml", model);
        }

        [ValidateInput(false)]
        public ActionResult SaveDetails(ProjectDetails model)
        {
            if (ModelState.IsValid == false)
                return CurrentUmbracoPage();

            var nodeListingProvider = new NodeListingProvider();
            var project = (model.Id != 0) ? nodeListingProvider.GetListing(model.Id) : new ListingItem(null, null);

            project.Name = model.Title;
            project.Description = model.Description;
            project.CurrentVersion = model.Version;
            project.LicenseName = model.License;
            project.LicenseUrl = model.LicenseUrl;
            project.ProjectUrl = model.ProjectUrl;
            project.SourceCodeUrl = model.SourceCodeUrl;
            project.DemonstrationUrl = model.DemonstrationUrl;
            project.CategoryId = int.Parse(model.Category);
            project.OpenForCollab = model.OpenForCollaboration;
            project.GACode = model.GoogleAnalyticsCode;
            project.ProjectGuid = (model.Guid == Guid.Empty) ? Guid.NewGuid() : model.Guid; //this is used as the Unique project ID.
            project.ListingType = ListingType.free;
            project.VendorId = Members.GetCurrentMemberId();

            project.TermsAgreementDate = DateTime.Now.ToUniversalTime();

            nodeListingProvider.SaveOrUpdate(project);

            return Redirect("/member/profile/projects/edit?editorStep=files&id=" + project.Id);
        }
    }
}
