using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using OurUmbraco.Forum.Extensions;
using OurUmbraco.MarketPlace.Interfaces;
using OurUmbraco.MarketPlace.ListingItem;
using OurUmbraco.MarketPlace.NodeListing;
using OurUmbraco.MarketPlace.Providers;
using OurUmbraco.Our.Models;
using OurUmbraco.Project.Helpers;
using OurUmbraco.Wiki.BusinessLogic;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.Controllers
{
    public class ProjectController : SurfaceController
    {
        private string _exceptionName = "uIntra";

        [ChildActionOnly]
        public ActionResult Index(int projectId = 0)
        {
            var project = GetProjectForAuthorizedMember(projectId);

            var model = new ProjectDetails();
            var currentPage = Umbraco.TypedContent(UmbracoContext.Current.PageId);
            var rootNode = currentPage.AncestorOrSelf(1);
            var projects = rootNode.Children(x => x.ContentType.Alias == "Projects").First();
            var categories = projects.Children(x => x.ContentType.Alias == "ProjectGroup" && x.GetPropertyValue<bool>("hqOnly") == false);
            model.ProjectCategories = new List<SelectListItem> { new SelectListItem { Text = string.Empty, Value = string.Empty } };
            foreach (var category in categories)
                model.ProjectCategories.Add(new SelectListItem { Text = category.Name, Value = category.Id.ToString(), Selected = project.CategoryId == category.Id });

            model.License = string.IsNullOrWhiteSpace(project.LicenseName) ? "MIT" : project.LicenseName;
            model.LicenseUrl = string.IsNullOrWhiteSpace(project.LicenseUrl) ? "http://www.opensource.org/licenses/MIT" : project.LicenseUrl;

            model.Title = project.Name;
            model.Description = project.Description;
            model.Version = project.CurrentVersion;
            model.ProjectUrl = project.ProjectUrl;
            model.SourceCodeUrl = project.SourceCodeUrl;
            model.NuGetPackageUrl = project.NuGetPackageUrl;
            model.BugTrackingUrl = project.SupportUrl;
            model.DemonstrationUrl = project.DemonstrationUrl;
            model.OpenForCollaboration = project.OpenForCollab;
            model.GoogleAnalyticsCode = project.GACode;
            model.Id = projectId;

            return PartialView("~/Views/Partials/Project/Edit.cshtml", model);
        }

        private PublishedContentListingItem GetProjectForAuthorizedMember(int projectId)
        {
            //TODO: What if this is not found??! 
            var project = new PublishedContentListingItem();
            if (projectId != 0)
            {
                var nodeListingProvider = new NodeListingProvider();
                project = (PublishedContentListingItem)nodeListingProvider.GetListing(projectId);

                var member = Members.GetCurrentMember();
                if (member.IsHq())
                    return project;

                // If the member is not the owner of the project and not a contributor then they can not edit.
                if (project.VendorId != member.Id && Utils.IsProjectContributor(member.Id, projectId) == false)
                {
                    //TODO: Ummm... this is a child action/partial view - you cannot redirect from here
                    Response.Redirect("/member/profile/projects/", true);
                }
            }
            return project;
        }

        [ValidateInput(false)]
        public ActionResult SaveDetails(ProjectDetails model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            var nodeListingProvider = new NodeListingProvider();
            var project = (model.Id != 0) ? nodeListingProvider.GetListing(model.Id) : new PublishedContentListingItem();

            project.Name = model.Title;
            project.Description = model.Description;
            project.CurrentVersion = model.Version;
            project.LicenseName = model.License;
            project.LicenseUrl = model.LicenseUrl;
            project.ProjectUrl = model.ProjectUrl;
            project.SourceCodeUrl = model.SourceCodeUrl;
            project.NuGetPackageUrl = model.NuGetPackageUrl;
            project.SupportUrl = model.BugTrackingUrl;
            project.DemonstrationUrl = model.DemonstrationUrl;
            project.CategoryId = int.Parse(model.Category);
            project.OpenForCollab = model.OpenForCollaboration;
            project.GACode = model.GoogleAnalyticsCode;
            project.ProjectGuid = (model.Guid == Guid.Empty) ? Guid.NewGuid() : model.Guid; //this is used as the Unique project ID.
            project.ListingType = ListingType.free;
            project.IsRetired = model.IsRetired;
            project.RetiredMessage = model.RetiredMessage;

            // only set memberId when saving for the first time, else collaborators will cause it to switch the owner of the package
            if (model.Id == 0)
                project.VendorId = Members.GetCurrentMemberId();

            project.TermsAgreementDate = DateTime.Now.ToUniversalTime();

            nodeListingProvider.SaveOrUpdate(project);

            return Redirect("/member/profile/projects/edit?editorStep=files&id=" + project.Id);
        }

        [ChildActionOnly]
        public ActionResult RenderFiles(int id)
        {
            var project = GetProjectForAuthorizedMember(id);

            var mediaProvider = new MediaProvider();

            var availableFiles = mediaProvider.GetMediaFilesByProjectId(id)
                .Where(x => x.FileType != FileType.screenshot.FileTypeAsString()).ToList();

            foreach (var wikiFile in availableFiles)
                wikiFile.Current = project.CurrentReleaseFile == wikiFile.Id.ToString();

            var model = new EditFileModel
            {
                AvailableFiles = availableFiles,
                UploadFile = new WikiFileModel
                {
                    AvailableVersions = new List<SelectListItem>(WikiFileModel.GetUmbracoVersions())
                }
            };

            return PartialView("~/Views/Partials/Project/EditFiles.cshtml", model);
        }

        [ChildActionOnly]
        public ActionResult RenderScreenshots(int id)
        {
            // Getting this despite not using it to verify that the member owns this file
            var project = GetProjectForAuthorizedMember(id);

            var mediaProvider = new MediaProvider();

            var availableFiles = mediaProvider.GetMediaFilesByProjectId(id)
                .Where(x => x.FileType == FileType.screenshot.FileTypeAsString()).ToList();

            foreach (var wikiFile in availableFiles)
                wikiFile.Current = project.CurrentReleaseFile == wikiFile.Id.ToString();

            var model = new EditScreenshotModel
            {
                AvailableFiles = availableFiles,
                UploadFile = new ScreenshotModel()
            };

            return PartialView("~/Views/Partials/Project/EditScreenshots.cshtml", model);
        }

        [ChildActionOnly]
        public ActionResult RenderComplete(int id)
        {
            var project = GetProjectForAuthorizedMember(id);
            var nodeListingProvider = new NodeListingProvider();
            var packages = nodeListingProvider.GetMediaForProjectByType(project.Id, FileType.package);

            var errorMessage = string.Empty;
            var currentPackage = packages.FirstOrDefault(x => x.Current && x.Archived == false);

            // Special exception
            var isExceptionPackage = string.Equals(project.Name, _exceptionName, StringComparison.InvariantCultureIgnoreCase);

            if (isExceptionPackage == false && currentPackage == null)
                errorMessage = "None of the package files are marked as the current package, please make one current.";

            if (isExceptionPackage == false && currentPackage != null && ZipFileContainsPackageXml(currentPackage) == false)
            {
                var contentService = Services.ContentService;
                var content = contentService.GetById(project.Id);
                var projectIsLive = content.GetValue<bool>("projectLive");

                if (projectIsLive)
                {
                    content.SetValue("projectLive", false);
                    contentService.SaveAndPublishWithStatus(content);
                }
                errorMessage = string.Format("The current package file {0} is not a valid Umbraco Package, please upload a package", currentPackage.Name);
            }

            var model = new ProjectCompleteModel { Id = project.Id, Name = project.Name, ProjectLive = project.Live, ErrorMessage = errorMessage };

            return PartialView("~/Views/Partials/Project/Complete.cshtml", model);
        }

        private bool ZipFileContainsPackageXml(IMediaFile package)
        {
            var zipFile = IOHelper.MapPath(package.Path);
            try
            {
                LogHelper.Info<ProjectController>(string.Format("Checking if {0} has a package.xml zipped up in there.", zipFile));

                using (var archive = ZipFile.OpenRead(zipFile))
                {
                    var packageXmlFileExists = archive.Entries.Any(x => string.Equals(x.Name, "package.xml", StringComparison.InvariantCultureIgnoreCase));
                    if (packageXmlFileExists)
                        return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<ProjectController>(string.Format("Error unzipping {0}", zipFile), ex);
            }

            return false;
        }

        public ActionResult UpdateProjectLive(ProjectCompleteModel model)
        {
            var nodeListingProvider = new NodeListingProvider();
            var project = GetProjectForAuthorizedMember(model.Id);

            if (model.ProjectLive == false)
            {
                project.Live = false;
                nodeListingProvider.SaveOrUpdate(project);
            }
            else
            {
                // Special exception
                if (string.Equals(model.Name, _exceptionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    project.Live = true;
                    nodeListingProvider.SaveOrUpdate(project);
                }
                else
                {
                    var packages = nodeListingProvider.GetMediaForProjectByType(project.Id, FileType.package);
                    var currentPackage = packages.FirstOrDefault(x => x.Current && x.Archived == false);

                    if (currentPackage != null && ZipFileContainsPackageXml(currentPackage))
                    {
                        project.Live = true;
                        nodeListingProvider.SaveOrUpdate(project);
                    }
                }
            }

            return RedirectToCurrentUmbracoPage(Request.Url.Query);
        }

        public ActionResult MarkFileAsCurrent(int id, int releaseFileId)
        {
            var nodeListingProvider = new NodeListingProvider();
            var project = GetProjectForAuthorizedMember(id);
            project.CurrentReleaseFile = releaseFileId.ToString();
            var file = new WikiFile(releaseFileId);
            if (file.FileType == "screenshot")
                project.DefaultScreenshot = file.Path;
            nodeListingProvider.SaveOrUpdate(project);
            return RedirectToCurrentUmbracoPage(Request.Url.Query);
        }

        public ActionResult DeleteScreenshot(int id, int releaseFileId)
        {
            var nodeListingProvider = new NodeListingProvider();
            var project = GetProjectForAuthorizedMember(id);

            var file = new WikiFile(releaseFileId);
            if (file.Path == project.DefaultScreenshot)
            {
                project.DefaultScreenshot = string.Empty;
                nodeListingProvider.SaveOrUpdate(project);
            }

            var mediaProvider = new MediaProvider();
            mediaProvider.Remove(file);

            return RedirectToCurrentUmbracoPage(Request.Url.Query);
        }

        public ActionResult ArchiveFile(int id, int releaseFileId)
        {
            // Getting this despite not using it to verify that the member owns this file
            var project = GetProjectForAuthorizedMember(id);

            var mediaProvider = new MediaProvider();
            var releaseFile = mediaProvider.GetFileById(releaseFileId);
            releaseFile.Archived = !releaseFile.Archived;
            mediaProvider.SaveOrUpdate(releaseFile);

            return RedirectToCurrentUmbracoPage(Request.Url.Query);
        }

        public ActionResult AddFile(EditFileModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            // Getting this despite not using it to verify that the member owns this file
            var project = GetProjectForAuthorizedMember(model.UploadFile.ProjectId);
            var member = Members.GetCurrentMember();

            HttpPostedFile file;
            using (var target = new MemoryStream())
            {
                model.UploadFile.File.InputStream.CopyTo(target);
                byte[] data = target.ToArray();
                file = ConstructHttpPostedFile(data, model.UploadFile.File.FileName, model.UploadFile.File.ContentType);
            }

            var umbracoVersions = new List<UmbracoVersion>();
            var allUmbracoVersions = UmbracoVersion.AvailableVersions().Values;
            foreach (var item in model.UploadFile.SelectedVersions)
            {
                var version = allUmbracoVersions.Single(x => x.Version == item);
                umbracoVersions.Add(version);
            }

            var contentService = Services.ContentService;
            var projectContent = contentService.GetById(project.Id);

            var wikiFile = WikiFile.Create(
                model.UploadFile.File.FileName,
                projectContent.PublishedVersionGuid,
                member.GetKey(),
                file,
                model.UploadFile.FileType,
                umbracoVersions,
                model.UploadFile.DotNetVersion
            );

            return RedirectToCurrentUmbracoPage(Request.Url.Query);
        }

        public ActionResult AddScreenshot(EditScreenshotModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            // Getting this despite not using it to verify that the member owns this file
            var project = GetProjectForAuthorizedMember(model.UploadFile.ProjectId);
            var member = Members.GetCurrentMember();

            HttpPostedFile file;
            using (var target = new MemoryStream())
            {
                model.UploadFile.File.InputStream.CopyTo(target);
                byte[] data = target.ToArray();
                file = ConstructHttpPostedFile(data, model.UploadFile.File.FileName, model.UploadFile.File.ContentType);
            }

            var contentService = Services.ContentService;
            var projectContent = contentService.GetById(project.Id);

            var wikiFile = WikiFile.Create(
                model.UploadFile.File.FileName,
                projectContent.PublishedVersionGuid,
                member.GetKey(),
                file,
                "screenshot",
                new List<UmbracoVersion> { UmbracoVersion.DefaultVersion() }
            );

            return RedirectToCurrentUmbracoPage(Request.Url.Query);
        }

        public HttpPostedFile ConstructHttpPostedFile(byte[] data, string filename, string contentType)
        {
            // Get the System.Web assembly reference
            Assembly systemWebAssembly = typeof(HttpPostedFileBase).Assembly;
            // Get the types of the two internal types we need
            Type typeHttpRawUploadedContent = systemWebAssembly.GetType("System.Web.HttpRawUploadedContent");
            Type typeHttpInputStream = systemWebAssembly.GetType("System.Web.HttpInputStream");

            // Prepare the signatures of the constructors we want.
            Type[] uploadedParams = { typeof(int), typeof(int) };
            Type[] streamParams = { typeHttpRawUploadedContent, typeof(int), typeof(int) };
            Type[] parameters = { typeof(string), typeof(string), typeHttpInputStream };

            // Create an HttpRawUploadedContent instance
            object uploadedContent = typeHttpRawUploadedContent
              .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, uploadedParams, null)
              .Invoke(new object[] { data.Length, data.Length });

            // Call the AddBytes method
            typeHttpRawUploadedContent
              .GetMethod("AddBytes", BindingFlags.NonPublic | BindingFlags.Instance)
              .Invoke(uploadedContent, new object[] { data, 0, data.Length });

            // This is necessary if you will be using the returned content (ie to Save)
            typeHttpRawUploadedContent
              .GetMethod("DoneAddingBytes", BindingFlags.NonPublic | BindingFlags.Instance)
              .Invoke(uploadedContent, null);

            // Create an HttpInputStream instance
            object stream = (Stream)typeHttpInputStream
              .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, streamParams, null)
              .Invoke(new object[] { uploadedContent, 0, data.Length });

            // Create an HttpPostedFile instance
            HttpPostedFile postedFile = (HttpPostedFile)typeof(HttpPostedFile)
              .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null)
              .Invoke(new object[] { filename, contentType, stream });

            return postedFile;
        }
    }
}
