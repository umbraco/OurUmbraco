using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using File = System.IO.File;

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
            AddNewPackageFormatToggleToPackages();
            AddBannerPurposeToggleToPackages();
            AddIsPromotedToggleToPackages();
            AddReleaseCandidatePostfixToRelease();
            AddCommunityBlogsRssTemplate();
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
        
        private void AddNewPackageFormatToggleToPackages()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path)) return;

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var projectContentType = contentTypeService.GetContentType("Project");
                var propertyTypeAlias = "isNuGetFormat";
                
                if (projectContentType != null && projectContentType.PropertyTypeExists(propertyTypeAlias) == false)
                {
                    var checkbox = new DataTypeDefinition("Umbraco.TrueFalse");
                    var checkboxPropertyType = new PropertyType(checkbox, propertyTypeAlias)
                    {
                        Name = "Is v9+ compatible?",
                        Description = "The package is in the new NuGet format and compatible with v9+"
                    };
                    projectContentType.AddPropertyType(checkboxPropertyType, "Project");
                    contentTypeService.Save(projectContentType);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);

            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }
        
        private void AddBannerPurposeToggleToPackages()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path)) return;

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var projectContentType = contentTypeService.GetContentType("banner");
                var propertyTypeAlias = "supportsCommunityProject";
                
                if (projectContentType != null && projectContentType.PropertyTypeExists(propertyTypeAlias) == false)
                {
                    var checkbox = new DataTypeDefinition("Umbraco.TrueFalse");
                    var checkboxPropertyType = new PropertyType(checkbox, propertyTypeAlias)
                    {
                        Name = "Supports a community project",
                        Description = "Banners are meant to support a community project (contact Ilham/Seb for questions)",
                        Mandatory = true
                    };
                    projectContentType.AddPropertyType(checkboxPropertyType, "Banner");
                    contentTypeService.Save(projectContentType);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);

            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddIsPromotedToggleToPackages()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path)) return;

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var projectContentType = contentTypeService.GetContentType("Project");
                var propertyTypeAlias = "isPromoted";

                if (projectContentType != null && projectContentType.PropertyTypeExists(propertyTypeAlias) == false)
                {
                    var checkbox = new DataTypeDefinition("Umbraco.TrueFalse");
                    var checkboxPropertyType = new PropertyType(checkbox, propertyTypeAlias)
                    {
                        Name = "Is Promoted?",
                        Description = "The package is selected for promotion (HQ, technical partner or selected community package)."
                    };
                    projectContentType.AddPropertyType(checkboxPropertyType, "Project");
                    contentTypeService.Save(projectContentType);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);

            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddReleaseCandidatePostfixToRelease()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path)) return;

                const string contentTypeAlias = "Release";
                const string propertyTypeAlias = "ReleaseCandidatePostfix";
                
                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var projectContentType = contentTypeService.GetContentType(contentTypeAlias);

                if (projectContentType != null && projectContentType.PropertyTypeExists(propertyTypeAlias) == false)
                {
                    var textBox = new DataTypeDefinition("Umbraco.Textbox");
                    var textBoxPropertyType = new PropertyType(textBox, propertyTypeAlias)
                    {
                        Name = "Release Candidate Postfix",
                        Description = "If this is an RC2, enter 2 in this field, if it's RC003 then enter 003"
                    };
                    projectContentType.AddPropertyType(textBoxPropertyType, propertyGroupName: "Release");
                    contentTypeService.Save(projectContentType);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);

            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }
        private void AddCommunityBlogsRssTemplate()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path)) return;

                const string templateAlias = "CommunityBlogsRss";
                
                var fileService = ApplicationContext.Current.Services.FileService;
                var template = fileService.GetTemplate(templateAlias);
                if (template == null)
                {
                    template = new Template(templateAlias, templateAlias);
                }

                var file = HostingEnvironment.MapPath("~/config/blogrsstemplate.txt");
                var content = File.ReadAllText(file);
                template.Content = content;
                fileService.SaveTemplate(template);
                
                string[] lines = { "" };
                File.WriteAllLines(path, lines);

            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }
    }
}