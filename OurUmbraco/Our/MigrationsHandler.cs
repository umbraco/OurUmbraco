using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
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
            AddSearchDocumentTypeAndPage();
            UseNewMyProjectsOverview();
            UseNewRssFeedsOverview();
            RenameUaaStoUCloud();
            AddMarkAsSolutionReminderSent();
            RenameUaaStoUCloud();
            UseNewLoginForm();
            UseNewForgotPasswordForm();
            AddTwitterFilters();
            AddHomeScriptsMacro();
            AddNuGetUrlForPackages();
            AddPeopleKarmaPage();
            AddCommunityPage();
            AddCommunityHubPage();
            AddCommunityBlogs();
            AddCommunityStatistics();
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

                var macroService = ApplicationContext.Current.Services.MacroService;
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

                var contentService = ApplicationContext.Current.Services.ContentService;
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

                var macroService = ApplicationContext.Current.Services.MacroService;
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

                var contentService = ApplicationContext.Current.Services.ContentService;
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

                var macroService = ApplicationContext.Current.Services.MacroService;
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

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
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

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
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

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
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

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
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

                var userService = ApplicationContext.Current.Services.UserService;
                var rootUser = userService.GetUserById(0);
                if (rootUser == null)
                    return;

                // Don't run this on Seb's database which has slightly different data in it
                if (rootUser.Email == "pph@umrbaco.org")
                    return;

                var db = ApplicationContext.Current.DatabaseContext.Database;
                db.Execute("DELETE FROM [umbracoUser] WHERE id != 0");
                db.Execute("DELETE FROM [umbracoUser2app] WHERE [user] != 0");
                db.Execute("DBCC CHECKIDENT ('dbo.umbracoUser');");
                db.Execute("DBCC CHECKIDENT ('dbo.umbracoUser', NORESEED);");
                db.Execute("DBCC CHECKIDENT ('dbo.umbracoUser', RESEED, 1);");

                for (var i = 0; i < 17; i++)
                {
                    var user = userService.CreateUserWithIdentity(string.Format("user{0}", i), string.Format("user{0}@test.com", i));

                    var userGroup = ToReadOnlyGroup(userService.GetUserGroupByAlias("admin"));
                    user.AddGroup(userGroup);
                    userService.Save(user);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        public static IReadOnlyUserGroup ToReadOnlyGroup(IUserGroup group)
        {
            //this will generally always be the case
            var readonlyGroup = group as IReadOnlyUserGroup;
            if (readonlyGroup != null) return readonlyGroup;

            //otherwise create one
            return new ReadOnlyUserGroup(group.Id, group.Name, group.Icon, group.StartContentId, group.StartMediaId, group.Alias, group.AllowedSections, group.Permissions);
        }


        private void AddReleaseCompareFeature()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
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

                var fileService = ApplicationContext.Current.Services.FileService;

                var masterTemplate = fileService.GetTemplate("Master");
                releaseCompareTemplate.SetMasterTemplate(masterTemplate);

                fileService.SaveTemplate(releaseCompareTemplate);
                contentTypeService.Save(compareContentType);

                compareContentType.AllowedTemplates = new List<ITemplate> { releaseCompareTemplate };
                compareContentType.SetDefaultTemplate(releaseCompareTemplate);

                contentTypeService.Save(compareContentType);

                var contentService = ApplicationContext.Current.Services.ContentService;
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

                var macroService = ApplicationContext.Current.Services.MacroService;
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

                var contentService = ApplicationContext.Current.Services.ContentService;
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

                var macroService = ApplicationContext.Current.Services.MacroService;
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

                var db = ApplicationContext.Current.DatabaseContext.Database;
                db.Execute("ALTER TABLE [wikiFiles] ADD [minimumVersionStrict] VARCHAR(50)");

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }


        private void AddSearchDocumentTypeAndPage()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var searchContentType = contentTypeService.GetContentType("search");
                if (searchContentType == null)
                {
                    var contentType = new ContentType(-1)
                    {
                        Name = "Search",
                        Alias = "search",
                        Icon = "icon-search"
                    };
                    contentType.PropertyGroups.Add(new PropertyGroup { Name = "Content" });

                    var checkbox = new DataTypeDefinition("Umbraco.TrueFalse");
                    var checkboxPropertyType = new PropertyType(checkbox, "umbracoNaviHide") { Name = "Hide in navigation?" };
                    contentType.AddPropertyType(checkboxPropertyType, "Content");

                    contentTypeService.Save(contentType);

                    searchContentType = contentTypeService.GetContentType("search");
                    var templateCreateResult = ApplicationContext.Current.Services.FileService.CreateTemplateForContentType("search", "Search");
                    if (templateCreateResult.Success)
                    {
                        var template = ApplicationContext.Current.Services.FileService.GetTemplate("search");
                        var masterTemplate = ApplicationContext.Current.Services.FileService.GetTemplate("master");
                        template.SetMasterTemplate(masterTemplate);
                        ApplicationContext.Current.Services.FileService.SaveTemplate(template);

                        searchContentType.AllowedTemplates = new List<ITemplate> { template };
                        searchContentType.SetDefaultTemplate(template);
                        contentTypeService.Save(searchContentType);

                        var contentService = ApplicationContext.Current.Services.ContentService;
                        var rootContent = contentService.GetRootContent().FirstOrDefault();
                        if (rootContent != null)
                        {
                            var searchPage = rootContent.Children().FirstOrDefault(x => x.Name == "Search");
                            contentService.Delete(searchPage);
                            searchPage = contentService.CreateContent("Search", rootContent.Id, "search");
                            var saveResult = contentService.SaveAndPublishWithStatus(searchPage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void UseNewMyProjectsOverview()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var macroService = ApplicationContext.Current.Services.MacroService;
                var allMacros = macroService.GetAll();
                //For some reason GetByAlias does not work
                var macro = allMacros.FirstOrDefault(x => x.Alias == "DeliMyProjects");
                macro.ControlType = "";
                macro.ScriptPath = "~/Views/MacroPartials/Projects/MyProjects.cshtml";
                macroService.Save(macro);

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void UseNewRssFeedsOverview()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var contentService = ApplicationContext.Current.Services.ContentService;
                var rootContent = contentService.GetRootContent().FirstOrDefault();
                if (rootContent != null)
                {
                    var rssPage = rootContent.Children().FirstOrDefault(x => string.Equals(x.Name, "Rss", StringComparison.InvariantCultureIgnoreCase));
                    foreach (var page in rssPage.Children())
                    {
                        if (string.Equals(page.Name, "Wiki", StringComparison.InvariantCultureIgnoreCase))
                            contentService.UnPublish(page);

                        if (string.Equals(page.Name, "CommunityBlogs", StringComparison.InvariantCultureIgnoreCase))
                            contentService.UnPublish(page);

                        if (string.Equals(page.Name, "Help", StringComparison.InvariantCultureIgnoreCase))
                            contentService.UnPublish(page);


                        if (string.Equals(page.Name, "Forum", StringComparison.InvariantCultureIgnoreCase))
                        {
                            page.SetValue("macro", "RSSForum");
                            contentService.SaveAndPublishWithStatus(page);
                        }

                        if (string.Equals(page.Name, "ActiveTopics", StringComparison.InvariantCultureIgnoreCase))
                        {
                            page.SetValue("macro", "RssLatestTopics");
                            contentService.SaveAndPublishWithStatus(page);
                        }

                        if (string.Equals(page.Name, "Projects", StringComparison.InvariantCultureIgnoreCase))
                        {
                            page.SetValue("macro", "RSSPackages");
                            contentService.SaveAndPublishWithStatus(page);
                        }

                        if (string.Equals(page.Name, "Karma", StringComparison.InvariantCultureIgnoreCase))
                            contentService.UnPublish(page);

                        if (string.Equals(page.Name, "YourTopics", StringComparison.InvariantCultureIgnoreCase))
                        {
                            page.SetValue("macro", "RSSParticipated");
                            contentService.SaveAndPublishWithStatus(page);
                        }

                        if (string.Equals(page.Name, "ProjectsUpdate", StringComparison.InvariantCultureIgnoreCase))
                        {
                            page.SetValue("macro", "RssPackages");
                            contentService.SaveAndPublishWithStatus(page);
                        }

                        if (string.Equals(page.Name, "Topic", StringComparison.InvariantCultureIgnoreCase))
                        {
                            page.SetValue("macro", "RSSTopic");
                            contentService.SaveAndPublishWithStatus(page);
                        }
                    }

                    var htmlPage = rootContent.Children().FirstOrDefault(x => string.Equals(x.Name, "html", StringComparison.InvariantCultureIgnoreCase));
                    foreach (var page in htmlPage.Children())
                    {
                        if (string.Equals(page.Name, "GithubPullTrigger", StringComparison.InvariantCultureIgnoreCase))
                        {
                            page.SetValue("macro", "Documentation-GithubSync");
                            contentService.SaveAndPublishWithStatus(page);
                        }
                        else
                        {
                            contentService.UnPublish(page);
                        }
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
        private void RenameUaaStoUCloud()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var contentService = ApplicationContext.Current.Services.ContentService;
                var rootContent = contentService.GetRootContent().FirstOrDefault();
                if (rootContent != null)
                {
                    var forumContent = rootContent.Children().FirstOrDefault(x => x.Name == "Forum");
                    if (forumContent != null)
                    {
                        var uaasForumContent = forumContent.Children().FirstOrDefault(x => x.Name.ToLowerInvariant() == "Umbraco as a Service".ToLowerInvariant());
                        if (uaasForumContent != null)
                        {
                            uaasForumContent.Name = "Umbraco Cloud";
                            uaasForumContent.SetValue("forumDescription", "Discussions about Umbraco Cloud.");
                            var status = contentService.SaveAndPublishWithStatus(uaasForumContent);
                            status = contentService.SaveAndPublishWithStatus(uaasForumContent);
                        }
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

        private void AddMarkAsSolutionReminderSent()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var db = ApplicationContext.Current.DatabaseContext.Database;
                db.Execute("ALTER TABLE [forumTopics] ADD [markAsSolutionReminderSent] [BIT] NULL DEFAULT ((0))");

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void UseNewLoginForm()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var macroService = ApplicationContext.Current.Services.MacroService;
                var macro = macroService.GetByAlias("MemberLogin");
                macro.ControlType = "";
                macro.ScriptPath = "~/Views/MacroPartials/Members/Login.cshtml";
                macroService.Save(macro);

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void UseNewForgotPasswordForm()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var macroService = ApplicationContext.Current.Services.MacroService;
                var macro = macroService.GetByAlias("MemberPasswordReminder");
                macro.ControlType = "";
                macro.ScriptPath = "~/Views/MacroPartials/Members/ForgotPassword.cshtml";
                macroService.Save(macro);

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }


        private void AddTwitterFilters()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var communityContentType = contentTypeService.GetContentType("Community");
                var propertyTypeAlias = "twitterFilterAccounts";
                var textboxMultiple = new DataTypeDefinition("Umbraco.TextboxMultiple");

                var tabName = "Settings";
                if (communityContentType.PropertyGroups.Contains(tabName) == false)
                    communityContentType.AddPropertyGroup(tabName);

                if (communityContentType.PropertyTypeExists(propertyTypeAlias) == false)
                {
                    var textboxAccountsFilter = new PropertyType(textboxMultiple, propertyTypeAlias) { Name = "CSV of Twitter accounts to filter" };
                    communityContentType.AddPropertyType(textboxAccountsFilter, tabName);
                }

                propertyTypeAlias = "twitterFilterWords";
                if (communityContentType.PropertyTypeExists(propertyTypeAlias) == false)
                {
                    var textboxWordFilter = new PropertyType(textboxMultiple, propertyTypeAlias) { Name = "CSV of words filter tweets out" };
                    communityContentType.AddPropertyType(textboxWordFilter, tabName);
                }

                contentTypeService.Save(communityContentType);

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }


        private void AddHomeScriptsMacro()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var macroService = ApplicationContext.Current.Services.MacroService;
                var macroAlias = "CommunityHomeScripts";
                if (macroService.GetByAlias(macroAlias) == null)
                {
                    // Run migration

                    var macro = new Macro
                    {
                        Name = "[Community] Home Scripts",
                        Alias = macroAlias,
                        ScriptingFile = "~/Views/MacroPartials/Community/Scripts.cshtml",
                        UseInEditor = false
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

        private void AddNuGetUrlForPackages()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var projectContentType = contentTypeService.GetContentType("Project");
                var propertyTypeAlias = "nuGetPackageUrl";
                var textbox = new DataTypeDefinition("Umbraco.Textbox");

                var tabName = "Project";
                if (projectContentType.PropertyGroups.Contains(tabName) == false)
                    projectContentType.AddPropertyGroup(tabName);

                if (projectContentType.PropertyTypeExists(propertyTypeAlias) == false)
                {
                    var textboxNuGetPackage = new PropertyType(textbox, propertyTypeAlias);
                    projectContentType.AddPropertyType(textboxNuGetPackage, tabName);
                }

                contentTypeService.Save(projectContentType);

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddPeopleKarmaPage()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;


                var templatePathRelative = "~/Views/PeopleKarma.cshtml";
                var templatePath = HostingEnvironment.MapPath(templatePathRelative);
                var templateContent = File.ReadAllText(templatePath);
                var releaseCompareTemplate = new Template("PeopleKarma", "PeopleKarma")
                {
                    MasterTemplateAlias = "Master",
                    Content = templateContent
                };

                var fileService = ApplicationContext.Current.Services.FileService;

                var masterTemplate = fileService.GetTemplate("Master");
                releaseCompareTemplate.SetMasterTemplate(masterTemplate);

                fileService.SaveTemplate(releaseCompareTemplate);


                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddCommunityPage()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var communityHubContentType = contentTypeService.GetContentType("communityHub");
                if (communityHubContentType == null)
                {
                    var contentType = new ContentType(-1)
                    {
                        Name = "CommunityHub",
                        Alias = "communityHub",
                        Icon = "icon-power"
                    };
                    contentType.PropertyGroups.Add(new PropertyGroup { Name = "Content" });

                    var checkbox = new DataTypeDefinition("Umbraco.TrueFalse");
                    var checkboxPropertyType = new PropertyType(checkbox, "umbracoNaviHide") { Name = "Hide in navigation?" };
                    contentType.AddPropertyType(checkboxPropertyType, "Content");

                    contentTypeService.Save(contentType);

                    communityHubContentType = contentTypeService.GetContentType("communityHub");
                    var templateCreateResult = ApplicationContext.Current.Services.FileService.CreateTemplateForContentType("communityHub", "CommunityHub");
                    if (templateCreateResult.Success)
                    {
                        var template = ApplicationContext.Current.Services.FileService.GetTemplate("communityHub");
                        var masterTemplate = ApplicationContext.Current.Services.FileService.GetTemplate("master");
                        template.SetMasterTemplate(masterTemplate);
                        ApplicationContext.Current.Services.FileService.SaveTemplate(template);

                        communityHubContentType.AllowedTemplates = new List<ITemplate> { template };
                        communityHubContentType.SetDefaultTemplate(template);
                        contentTypeService.Save(communityHubContentType);

                        var contentService = ApplicationContext.Current.Services.ContentService;
                        var rootContent = contentService.GetRootContent().FirstOrDefault();
                        if (rootContent != null)
                        {
                            var communityPage = rootContent.Children().FirstOrDefault(x => x.Name == "Community");
                            if (communityPage == null)
                            {
                                communityPage = contentService.CreateContent("Community", rootContent.Id, "communityHub");
                                communityPage.SetValue("umbracoNaviHide", true);
                                var saveResult = contentService.SaveAndPublishWithStatus(communityPage);
                            }
                        }
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

        private void AddCommunityHubPage()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var hubPageContentType = contentTypeService.GetContentType("communityHubPage");
                if (hubPageContentType == null)
                {
                    var contentType = new ContentType(-1)
                    {
                        Name = "Community Hub Page",
                        Alias = "communityHubPage",
                        Icon = "icon-wifi"
                    };
                    contentType.PropertyGroups.Add(new PropertyGroup { Name = "Content" });

                    var checkbox = new DataTypeDefinition("Umbraco.TrueFalse");
                    var checkboxPropertyType = new PropertyType(checkbox, "umbracoNaviHide") { Name = "Hide in navigation?" };
                    contentType.AddPropertyType(checkboxPropertyType, "Content");

                    var rte = new DataTypeDefinition("Umbraco.TinyMCEv3");
                    var rtePropertyType = new PropertyType(rte, "bodyText") { Name = "Body" };
                    contentType.AddPropertyType(rtePropertyType, "Content");

                    contentTypeService.Save(contentType);

                    hubPageContentType = contentTypeService.GetContentType("communityHubPage");

                    var communityHubContentType = contentTypeService.GetContentType("communityHub");
                    if (communityHubContentType != null)
                    {
                        var allowedContentTypes = new List<ContentTypeSort> { new ContentTypeSort(contentType.Id, 0) };
                        communityHubContentType.AllowedContentTypes = allowedContentTypes;
                        contentTypeService.Save(communityHubContentType);
                    }

                    var templateCreateResult = ApplicationContext.Current.Services.FileService.CreateTemplateForContentType("communityHubPage", "CommunityHubPage");
                    if (templateCreateResult.Success)
                    {
                        var template = ApplicationContext.Current.Services.FileService.GetTemplate("communityHubPage");
                        var masterTemplate = ApplicationContext.Current.Services.FileService.GetTemplate("master");
                        template.SetMasterTemplate(masterTemplate);
                        ApplicationContext.Current.Services.FileService.SaveTemplate(template);

                        hubPageContentType.AllowedTemplates = new List<ITemplate> { template };
                        hubPageContentType.SetDefaultTemplate(template);
                        contentTypeService.Save(hubPageContentType);
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

        private void AddCommunityBlogs()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                const string templateName = "CommunityBlogs";
                const string contentItemName = "Blogs";
                CreateNewCommunityHubPage(templateName, contentItemName);

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }


        private void AddCommunityStatistics()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                const string templateName = "CommunityStatistics";
                const string contentItemName = "Statistics";
                CreateNewCommunityHubPage(templateName, contentItemName);

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private static void CreateNewCommunityHubPage(string templateName, string contentItemName)
        {
            var relativeTemplateLocation = $"~/Views/{templateName}.cshtml";

            var hubNode = GetCommunityHubNode();
            if (hubNode != null)
            {
                var templateContents = string.Empty;

                var templateFile = HostingEnvironment.MapPath(relativeTemplateLocation);
                if (templateFile != null && File.Exists(templateFile))
                    templateContents = File.ReadAllText(templateFile);

                var templateCreateResult =
                    ApplicationContext.Current.Services.FileService.CreateTemplateForContentType("communityHubPage",
                        templateName);
                if (templateCreateResult.Success)
                {
                    var template = ApplicationContext.Current.Services.FileService.GetTemplate(templateName);
                    var masterTemplate = ApplicationContext.Current.Services.FileService.GetTemplate("master");
                    template.SetMasterTemplate(masterTemplate);
                    if (templateContents != string.Empty)
                        template.Content = templateContents;
                    ApplicationContext.Current.Services.FileService.SaveTemplate(template);

                    var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                    var hubPageContentType = contentTypeService.GetContentType("communityHubPage");
                    var allowedTemplates = new List<ITemplate> {template};
                    allowedTemplates.AddRange(hubPageContentType.AllowedTemplates);
                    hubPageContentType.AllowedTemplates = allowedTemplates;
                    contentTypeService.Save(hubPageContentType);
                }

                var contentService = ApplicationContext.Current.Services.ContentService;
                var hubPage = contentService.CreateContent(contentItemName, hubNode.Id, "communityHubPage");
                hubPage.SetValue("umbracoNaviHide", false);
                hubPage.Template = ApplicationContext.Current.Services.FileService.GetTemplate(templateName);
                var saveResult = contentService.SaveAndPublishWithStatus(hubPage);
            }
        }

        private static IContent GetCommunityHubNode()
        {
            var contentService = ApplicationContext.Current.Services.ContentService;
            var rootContent = contentService.GetRootContent().FirstOrDefault();
            return rootContent != null ? rootContent.Children().FirstOrDefault(x => x.Name == "Community") : null;
        }
    }
}