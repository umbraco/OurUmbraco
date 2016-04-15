using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;
using File = System.IO.File;
using Macro = umbraco.cms.businesslogic.macro.Macro;

namespace OurUmbraco.Our
{
    public class MigrationsHandler : ApplicationEventHandler
    {
        private const string MigrationMarkersPath = "~/App_Data/migrations/";
        
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            EnsureMigrationsMarkerPathExists();
            MemberActivationMigration();
            SpamOverview();
            CommunityHome();
            OverrideYouTrack();
            UaaSProjectCheckbox();
            ForumArchivedCheckbox();
            AddHomeOnlyBannerTextArea();
        }

        private void EnsureMigrationsMarkerPathExists()
        {
            var path = HostingEnvironment.MapPath(MigrationMarkersPath);
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);
        }

        private void MemberActivationMigration()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var macroService = UmbracoContext.Current.Application.Services.MacroService;
                var macroAlias = "MembersActivate";
                if (macroService.GetByAlias(macroAlias) == null)
                {
                    // Run migration
                    
                    var macro = new Macro
                    {
                        Name = "[Members] Activate",
                        Alias = macroAlias,
                        ScriptingFile = "~/Views/MacroPartials/Members/Activate.cshtml",
                        UseInEditor = true
                    };
                    macro.Save();
                }

                var contentService = UmbracoContext.Current.Application.Services.ContentService;
                var rootNode = contentService.GetRootContent().OrderBy(x => x.SortOrder).First(x => x.ContentType.Alias == "Community");

                var memberNode = rootNode.Children().FirstOrDefault(x => x.Name == "Member");

                var pendingActivationPageName = "Pending activation";
                if (memberNode != null && memberNode.Children().Any(x => x.Name == pendingActivationPageName) == false)
                {
                    var pendingActivationPage = contentService.CreateContent(pendingActivationPageName, memberNode.Id, "Textpage");
                    pendingActivationPage.SetValue("bodyText", "<p>Thanks for signing up! <br />We\'ve sent you an email containing an activation link.</p><p>To be able to continue you need to click the link in that email. If you didn\'t get any mail from us, make sure to check your spam/junkmail folder for mail from robot@umbraco.org.</p>");
                    contentService.SaveAndPublishWithStatus(pendingActivationPage);
                }

                var activatePageName = "Activate";
                if (memberNode != null && memberNode.Children().Any(x => x.Name == activatePageName) == false)
                {
                    var activatePage = contentService.CreateContent(activatePageName, memberNode.Id, "Textpage");
                    activatePage.SetValue("bodyText", string.Format("<?UMBRACO_MACRO macroAlias=\"{0}\" />", macroAlias));
                    contentService.SaveAndPublishWithStatus(activatePage);
                }
                
                string[] lines = {""};
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void SpamOverview()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var macroService = UmbracoContext.Current.Application.Services.MacroService;
                var macroAlias = "AntiSpam";
                if (macroService.GetByAlias(macroAlias) == null)
                {
                    // Run migration

                    var macro = new Macro
                    {
                        Name = "[Spam] Overview",
                        Alias = macroAlias,
                        ScriptingFile = "~/Views/MacroPartials/Spam/Overview.cshtml",
                        UseInEditor = true
                    };
                    macro.Save();
                }

                var contentService = UmbracoContext.Current.Application.Services.ContentService;
                var rootNode = contentService.GetRootContent().OrderBy(x => x.SortOrder).First(x => x.ContentType.Alias == "Community");

                var antiSpamPageName = "AntiSpam";
                if(rootNode.Children().Any(x => x.Name == antiSpamPageName) == false)
                {
                    var content = contentService.CreateContent(antiSpamPageName, rootNode.Id, "Textpage");
                    content.SetValue("bodyText", string.Format("<?UMBRACO_MACRO macroAlias=\"{0}\" />", macroAlias));
                    contentService.SaveAndPublishWithStatus(content);
                }
                
                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void CommunityHome()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var macroService = UmbracoContext.Current.Application.Services.MacroService;
                var macroAlias = "CommunityHome";
                if (macroService.GetByAlias(macroAlias) == null)
                {
                    // Run migration

                    var macro = new Macro
                    {
                        Name = "[Community] Home",
                        Alias = macroAlias,
                        ScriptingFile = "~/Views/MacroPartials/Community/Home.cshtml",
                        UseInEditor = true
                    };
                    macro.Save();
                }
                
                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void OverrideYouTrack()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var contentTypeService = UmbracoContext.Current.Application.Services.ContentTypeService;
                var releaseContentType = contentTypeService.GetContentType("Release");
                var propertyTypeAlias = "overrideYouTrackDescription";
                if (releaseContentType.PropertyTypeExists(propertyTypeAlias) == false)
                {
                    var checkbox = new DataTypeDefinition("Umbraco.TrueFalse");
                    var checkboxPropertyType = new PropertyType(checkbox, propertyTypeAlias) { Name = "Override YouTrack Description?" };
                    releaseContentType.AddPropertyType(checkboxPropertyType, "Content");
                    contentTypeService.Save(releaseContentType);
                }

                propertyTypeAlias = "overrideYouTrackReleaseDate";
                if (releaseContentType.PropertyTypeExists(propertyTypeAlias) == false)
                {
                    var textbox = new DataTypeDefinition("Umbraco.Textbox");
                    var textboxPropertyType = new PropertyType(textbox, propertyTypeAlias) { Name = "Override YouTrack release date" };
                    releaseContentType.AddPropertyType(textboxPropertyType, "Content");
                    contentTypeService.Save(releaseContentType);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void UaaSProjectCheckbox()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var contentTypeService = UmbracoContext.Current.Application.Services.ContentTypeService;
                var projectContentType = contentTypeService.GetContentType("Project");
                var propertyTypeAlias = "worksOnUaaS";
                if (projectContentType.PropertyTypeExists(propertyTypeAlias) == false)
                {
                    var checkbox = new DataTypeDefinition("Umbraco.TrueFalse");
                    var checkboxPropertyType = new PropertyType(checkbox, propertyTypeAlias) { Name = "Works on Umbraco as a Service?" };
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

        private void ForumArchivedCheckbox()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var contentTypeService = UmbracoContext.Current.Application.Services.ContentTypeService;
                var projectContentType = contentTypeService.GetContentType("Forum");
                var propertyTypeAlias = "archived";
                if (projectContentType.PropertyTypeExists(propertyTypeAlias) == false)
                {
                    var checkbox = new DataTypeDefinition("Umbraco.TrueFalse");
                    var checkboxPropertyType = new PropertyType(checkbox, propertyTypeAlias) { Name = "Archived" };
                    projectContentType.AddPropertyType(checkboxPropertyType, "Forum Information");
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

        private void AddHomeOnlyBannerTextArea()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var contentTypeService = UmbracoContext.Current.Application.Services.ContentTypeService;
                var communityContentType = contentTypeService.GetContentType("Community");
                var propertyTypeAlias = "homeOnlyBanner";
                if (communityContentType.PropertyTypeExists(propertyTypeAlias) == false)
                {
                    var textarea = new DataTypeDefinition("Umbraco.TextboxMultiple");
                    var textareaPropertyType = new PropertyType(textarea, propertyTypeAlias) { Name = "Banner (only shown on home)" };
                    communityContentType.AddPropertyType(textareaPropertyType, "Banners");
                    communityContentType.MovePropertyType("mainNotification", "Banners");
                    contentTypeService.Save(communityContentType);
                }
                
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
