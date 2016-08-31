using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            AddMissingUmbracoUsers();
            AddReleaseCompareFeature();
            AddTermsAndConditionsPage();
            UseNewRegistrationForm();
            AddStrictMinimumVersionForPackages();
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

                string[] lines = { "" };
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
                if (rootNode.Children().Any(x => x.Name == antiSpamPageName) == false)
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

        private void AddMissingUmbracoUsers()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var userService = UmbracoContext.Current.Application.Services.UserService;
                var rootUser = userService.GetUserById(0);
                if (rootUser == null)
                    return;

                // Don't run this on Seb's database which has slightly different data in it
                if (rootUser.Email == "pph@umrbaco.org")
                    return;

                var db = UmbracoContext.Current.Application.DatabaseContext.Database;
                db.Execute("DELETE FROM [umbracoUser] WHERE id != 0");
                db.Execute("DELETE FROM [umbracoUser2app] WHERE [user] != 0");
                db.Execute("DBCC CHECKIDENT ('dbo.umbracoUser');");
                db.Execute("DBCC CHECKIDENT ('dbo.umbracoUser', NORESEED);");
                db.Execute("DBCC CHECKIDENT ('dbo.umbracoUser', RESEED, 1);");

                for (var i = 0; i < 17; i++)
                {
                    var userType = userService.GetUserTypeByAlias("admin");
                    userService.CreateUserWithIdentity(string.Format("user{0}", i), string.Format("user{0}@test.com", i), userType);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }


        private void AddReleaseCompareFeature()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var contentTypeService = UmbracoContext.Current.Application.Services.ContentTypeService;
                var releaseCompareAlias = "ReleaseCompare";
                var compareContentType = contentTypeService.GetContentType(releaseCompareAlias);
                if (compareContentType == null)
                {
                    var contentType = new ContentType(-1)
                    {
                        Name = "Release Compare",
                        Alias = releaseCompareAlias
                    };
                    contentTypeService.Save(contentType);
                }

                compareContentType = contentTypeService.GetContentType(releaseCompareAlias);

                var releaseLandingContentType = contentTypeService.GetContentType("ReleaseLanding");
                
                var allowedContentTypes = new List<ContentTypeSort> { new ContentTypeSort(compareContentType.Id, 0) };
                releaseLandingContentType.AllowedContentTypes = allowedContentTypes;
                contentTypeService.Save(releaseLandingContentType);

                var templatePathRelative = "~/masterpages/ReleaseCompare.master";
                var templatePath = HostingEnvironment.MapPath(templatePathRelative);
                var templateContent = File.ReadAllText(templatePath);
                var releaseCompareTemplate = new Template("Release Compare", releaseCompareAlias)
                {
                    MasterTemplateAlias = "Master",
                    Content = templateContent
                };

                var fileService = UmbracoContext.Current.Application.Services.FileService;

                var masterTemplate = fileService.GetTemplate("Master");
                releaseCompareTemplate.SetMasterTemplate(masterTemplate);

                fileService.SaveTemplate(releaseCompareTemplate);
                contentTypeService.Save(compareContentType);

                compareContentType.AllowedTemplates = new List<ITemplate> { releaseCompareTemplate };
                compareContentType.SetDefaultTemplate(releaseCompareTemplate);

                contentTypeService.Save(compareContentType);

                var contentService = UmbracoContext.Current.Application.Services.ContentService;
                var rootNode = contentService.GetRootContent().OrderBy(x => x.SortOrder).First(x => x.ContentType.Alias == "Community");
                if (rootNode == null)
                    return;

                var contributeNode = rootNode.Children().FirstOrDefault(x => x.Name == "Contribute");
                if (contributeNode == null)
                    return;

                var releasesNode = contributeNode.Children().FirstOrDefault(x => x.Name == "Releases");

                if (releasesNode == null)
                    return;

                var compareContent = contentService.CreateContent("Compare", releasesNode.Id, "ReleaseCompare");
                compareContent.Template = releaseCompareTemplate;
                contentService.SaveAndPublishWithStatus(compareContent);

                var macroService = UmbracoContext.Current.Application.Services.MacroService;
                const string macroAlias = "ReleasesDropdown";
                if (macroService.GetByAlias(macroAlias) == null)
                {
                    var macro = new Macro
                    {
                        Name = "ReleasesDropdown",
                        Alias = macroAlias,
                        ScriptingFile = "~/Views/MacroPartials/Releases/ReleasesDropdown.cshtml",
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

        private void AddTermsAndConditionsPage()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var contentService = UmbracoContext.Current.Application.Services.ContentService;
                var rootContent = contentService.GetRootContent().FirstOrDefault();
                if (rootContent != null)
                {
                    var termsAndConditionsContent = rootContent.Children().FirstOrDefault(x => x.Name == "Terms and conditions");
                    if (termsAndConditionsContent == null)
                    {
                        var content = contentService.CreateContent("Terms and conditions", rootContent.Id, "TextPage");
                        content.SetValue("bodyText", "<p>These are our terms and conditions...:</p>");
                        var status = contentService.SaveAndPublishWithStatus(content);
                    }
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void UseNewRegistrationForm()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;
                
                var macroService = UmbracoContext.Current.Application.Services.MacroService;
                var macro = macroService.GetByAlias("MemberSignup");
                macro.ControlType = "";
                macro.ScriptPath = "~/Views/MacroPartials/Members/Register.cshtml";
                macroService.Save(macro);
                
                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddStrictMinimumVersionForPackages()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var db = UmbracoContext.Current.Application.DatabaseContext.Database;
                db.Execute("ALTER TABLE [wikiFiles] ADD [minimumVersionStrict] VARCHAR(50)");

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
