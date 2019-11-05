using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Linq;
using OurUmbraco.NotificationsCore;
using OurUmbraco.Our.Models.GitHub.AutoReplies;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
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
            AddCommunityVideos();
            AddCommunityBadges();
            AddCommunityCalendar();
            AddCommunityCalendarDocType();
            AddVideosPage();
            AddRetiredStatusToPackages();
            AddPasswordResetTokenToMembers();
            AddPRTeamPage();
            AddAdditionalUsers();
            AddReposDashboardPage();
            AddAppsDashboardPage();
            AddGitHubMemberProperties();
            AddTwitterMemberProperties();
            AddManualProgressSliderToReleases();
            AddSingleMediaPickerDataType();
            AddLocationDataTypes();
            AddBannerTypes();
            AddBannersPage();
            AddBannersPicker();
            AddBannersToCommunityPage();
            RemoveHomeOnlyBannerTextArea();
            ImportGitHubLabelDocTypes();
            CreateGitHubAutoRepliesTable();
            AddSmallRteDataType();
            AddMapToggleToCommunityHub();
            AddNotificationToCommunityHub();
            AddRtesToCommunityHub();
            AddCommunityBlogPostsTemplate();
            AddPackageLandingPageTemplate();
            AddEnhancedTextPage();
            AddEnhancedTextPageTemplate();
            AddGridDataType();
            AddGridToEnhancedTextPage();
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
                db.Execute("IF COL_LENGTH('wikiFiles', 'minimumVersionStrict') IS NULL BEGIN ALTER TABLE [wikiFiles] ADD [minimumVersionStrict] VARCHAR(50) END");

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
                db.Execute("IF COL_LENGTH('forumTopics', 'markAsSolutionReminderSent') IS NULL BEGIN ALTER TABLE [forumTopics] ADD [markAsSolutionReminderSent] [BIT] NULL DEFAULT ((0)) END");

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

        private void AddCommunityVideos()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                const string templateName = "CommunityVideos";
                const string contentItemName = "Videos";
                CreateNewCommunityHubPage(templateName, contentItemName);

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddCommunityBadges()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                const string templateName = "CommunityBadges";
                const string contentItemName = "Badges";
                CreateNewCommunityHubPage(templateName, contentItemName);

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddCommunityCalendar()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                const string templateName = "CommunityCalendar";
                const string contentItemName = "Calendar";
                CreateNewCommunityHubPage(templateName, contentItemName);

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void ImportGitHubLabelDocTypes()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var files = new List<string>();
                var basePath = HostingEnvironment.MapPath("~/App_Data/migrationsdata/");

                var datatypeFile = basePath + "0. GitHubDataType.xml";
                var xmlDocument = new XmlDocument { XmlResolver = null };
                xmlDocument.Load(datatypeFile);
                var element = XElement.Parse(xmlDocument.InnerXml);
                var importDataType =
                    ApplicationContext.Current.Services.PackagingService.ImportDataTypeDefinitions(element, 0);
                if (importDataType != null)
                {
                    files.Add(basePath + "1. GitHubLabelType.udt");
                    files.Add(basePath + "2. GitHubLabelComment.udt");
                    files.Add(basePath + "3. GitHubLabelCommentRepository.udt");
                    foreach (var file in files)
                    {
                        xmlDocument = new XmlDocument { XmlResolver = null };
                        xmlDocument.Load(file);

                        element = XElement.Parse(xmlDocument.InnerXml);
                        var importContentTypes =
                            ApplicationContext.Current.Services.PackagingService.ImportContentTypes(element, 0);
                        var contentType = importContentTypes.FirstOrDefault();
                        if (contentType != null)
                        {
                            //success
                        }
                    }

                    string[] lines = { "" };
                    File.WriteAllLines(path, lines);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddCommunityCalendarDocType()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                const string docTypeAlias = "calendarItem";
                const string templateName = "CalendarItem";

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;

                var calendarItemContentType = contentTypeService.GetContentType(docTypeAlias);
                if (calendarItemContentType == null)
                {
                    var contentType = new ContentType(-1)
                    {
                        Name = "Calendar Item",
                        Alias = docTypeAlias,
                        Icon = "icon-calendar"
                    };
                    contentType.PropertyGroups.Add(new PropertyGroup { Name = "Content" });

                    var textbox = new DataTypeDefinition("Umbraco.Textbox");
                    var textboxPropertyType = new PropertyType(textbox, "subTitle") { Name = "SubTitle" };
                    contentType.AddPropertyType(textboxPropertyType, "Content");

                    var dataTypeService = ApplicationContext.Current.Services.DataTypeService;
                    const string dataTypeName = "CalendarItemType";
                    if (dataTypeService.GetDataTypeDefinitionByName(dataTypeName) == null)
                    {
                        var dataType = new DataTypeDefinition(-1, "Umbraco.DropDown") { Name = dataTypeName };
                        dataTypeService.Save(dataType);
                        var dataTypeDefinition = dataTypeService.GetDataTypeDefinitionByName(dataTypeName);
                        var preValues = new Dictionary<string, PreValue>
                        {
                            { "Festival", new PreValue("Festival") },
                            { "MeetUp", new PreValue("MeetUp") },
                            { "Party", new PreValue("Party") },
                            { "Other", new PreValue("Other") }
                        };
                        dataTypeService.SavePreValues(dataTypeDefinition, preValues);
                    }

                    var dropdown = dataTypeService.GetDataTypeDefinitionByName(dataTypeName);
                    var dropdownPropertyType = new PropertyType(dropdown, "calendarItemType") { Name = "CalendarItemType" };
                    contentType.AddPropertyType(dropdownPropertyType, "Content");

                    var rte = new DataTypeDefinition("Umbraco.TinyMCEv3");
                    var rtePropertyType = new PropertyType(rte, "bodyText") { Name = "Description" };
                    contentType.AddPropertyType(rtePropertyType, "Content");

                    var dateTime = new DataTypeDefinition("Umbraco.DateTime");
                    var dateTimePropertyType = new PropertyType(dateTime, "start") { Name = "Start" };
                    contentType.AddPropertyType(dateTimePropertyType, "Content");

                    dateTime = new DataTypeDefinition("Umbraco.DateTime");
                    dateTimePropertyType = new PropertyType(dateTime, "end") { Name = "End" };
                    contentType.AddPropertyType(dateTimePropertyType, "Content");

                    textbox = new DataTypeDefinition("Umbraco.Textbox");
                    textboxPropertyType = new PropertyType(textbox, "location") { Name = "Location" };
                    contentType.AddPropertyType(textboxPropertyType, "Content");

                    textbox = new DataTypeDefinition("Umbraco.Textbox");
                    textboxPropertyType = new PropertyType(textbox, "url") { Name = "Url" };
                    contentType.AddPropertyType(textboxPropertyType, "Content");

                    var mediaPicker = new DataTypeDefinition("Umbraco.MediaPicker");
                    var mediaPickerPropertyType = new PropertyType(mediaPicker, "icon") { Name = "Icon" };
                    contentType.AddPropertyType(mediaPickerPropertyType, "Content");

                    contentTypeService.Save(contentType);

                    var hubPageContentType = contentTypeService.GetContentType("communityHubPage");
                    var allowedContentTypes = hubPageContentType.AllowedContentTypes.ToList();
                    var contentTypeId = contentTypeService.GetContentType(docTypeAlias).Id;
                    var contentTypeSort = new ContentTypeSort(contentTypeId, 0);
                    allowedContentTypes.Add(contentTypeSort);
                    hubPageContentType.AllowedContentTypes = allowedContentTypes;
                    contentTypeService.Save(hubPageContentType);
                }

                var relativeTemplateLocation = $"~/Views/{templateName}.cshtml";

                var templateContents = string.Empty;

                var templateFile = HostingEnvironment.MapPath(relativeTemplateLocation);
                if (templateFile != null && File.Exists(templateFile))
                    templateContents = File.ReadAllText(templateFile);

                var templateCreateResult =
                    ApplicationContext.Current.Services.FileService.CreateTemplateForContentType(docTypeAlias,
                        templateName);

                ITemplate template = null;
                if (templateCreateResult.Success)
                {
                    template = ApplicationContext.Current.Services.FileService.GetTemplate(templateName);
                    var masterTemplate = ApplicationContext.Current.Services.FileService.GetTemplate("master");
                    template.SetMasterTemplate(masterTemplate);
                    if (templateContents != string.Empty)
                        template.Content = templateContents;
                    ApplicationContext.Current.Services.FileService.SaveTemplate(template);
                    var docType = contentTypeService.GetContentType(docTypeAlias);
                    var allowedTemplates = new List<ITemplate> { template };
                    docType.AllowedTemplates = allowedTemplates;
                    contentTypeService.Save(docType);
                }

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
            var relativeTemplateLocation = string.Format("~/Views/{0}.cshtml", templateName);

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
                    var allowedTemplates = new List<ITemplate> { template };
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

        private void AddVideosPage()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            const string docTypeAlias = "videos";
            const string docTypeName = "Videos";

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var videosContentType = contentTypeService.GetContentType(docTypeAlias);
                if (videosContentType == null)
                {
                    var contentType = new ContentType(-1)
                    {
                        Name = docTypeName,
                        Alias = docTypeAlias,
                        Icon = "icon-power"
                    };
                    contentType.PropertyGroups.Add(new PropertyGroup { Name = "Content" });

                    var checkbox = new DataTypeDefinition("Umbraco.TrueFalse");
                    var checkboxPropertyType = new PropertyType(checkbox, "umbracoNaviHide") { Name = "Hide in navigation?" };
                    contentType.AddPropertyType(checkboxPropertyType, "Content");

                    contentTypeService.Save(contentType);

                    videosContentType = contentTypeService.GetContentType(docTypeAlias);
                    var templateCreateResult = ApplicationContext.Current.Services.FileService.CreateTemplateForContentType(docTypeAlias, docTypeName);
                    if (templateCreateResult.Success)
                    {
                        var template = ApplicationContext.Current.Services.FileService.GetTemplate(docTypeAlias);
                        var masterTemplate = ApplicationContext.Current.Services.FileService.GetTemplate("master");
                        template.SetMasterTemplate(masterTemplate);
                        ApplicationContext.Current.Services.FileService.SaveTemplate(template);

                        videosContentType.AllowedTemplates = new List<ITemplate> { template };
                        videosContentType.SetDefaultTemplate(template);
                        contentTypeService.Save(videosContentType);

                        var contentService = ApplicationContext.Current.Services.ContentService;
                        var rootContent = contentService.GetRootContent().FirstOrDefault();
                        if (rootContent != null)
                        {
                            var videosPage = rootContent.Children().FirstOrDefault(x => x.Name == "Videos");
                            if (videosPage == null)
                            {
                                videosPage = contentService.CreateContent("Videos", rootContent.Id, docTypeAlias);
                                videosPage.SetValue("umbracoNaviHide", true);
                                var saveResult = contentService.SaveAndPublishWithStatus(videosPage);
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

        private void AddRetiredStatusToPackages()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");

                if (File.Exists(path))
                {
                    return;
                }

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;

                var projectContentTypeAlias = "Project";
                var projectContentType = contentTypeService.GetContentType(projectContentTypeAlias);

                var checkboxTypeAlias = "isRetired";
                var textboxTypeAlias = "retiredMessage";

                if (projectContentType.PropertyTypeExists(checkboxTypeAlias) == false)
                {
                    var checkbox = new DataTypeDefinition("Umbraco.TrueFalse");
                    var checkboxPropertyType = new PropertyType(checkbox, checkboxTypeAlias) { Name = "Is Retired?" };
                    projectContentType.AddPropertyType(checkboxPropertyType, projectContentTypeAlias);
                    contentTypeService.Save(projectContentType);
                }

                if (projectContentType.PropertyTypeExists(textboxTypeAlias) == false)
                {
                    var textbox = new DataTypeDefinition("Umbraco.Textbox");
                    var textboxPropertyType = new PropertyType(textbox, textboxTypeAlias) { Name = "Retired Message" };
                    projectContentType.AddPropertyType(textboxPropertyType, projectContentTypeAlias);
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

        private void AddPasswordResetTokenToMembers()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");

                if (File.Exists(path))
                {
                    return;
                }

                var memberTypeService = ApplicationContext.Current.Services.MemberTypeService;

                var memberTypeAlias = "member";
                var memberType = memberTypeService.Get(memberTypeAlias);

                const string passwordResetAlias = "passwordResetToken";
                const string expiryDateAlias = "passwordResetTokenExpiryDate";

                var saveRequired = false;
                if (memberType.PropertyTypeExists(passwordResetAlias) == false)
                {
                    var textbox = new DataTypeDefinition("Umbraco.Textbox");
                    var textboxPropertyType = new PropertyType(textbox, passwordResetAlias) { Name = "Password reset token" };
                    memberType.AddPropertyType(textboxPropertyType, memberTypeAlias);
                    saveRequired = true;
                }

                if (memberType.PropertyTypeExists(expiryDateAlias) == false)
                {
                    var textbox = new DataTypeDefinition("Umbraco.Textbox");
                    var textboxPropertyType = new PropertyType(textbox, expiryDateAlias) { Name = "Password reset token expiry date" };
                    memberType.AddPropertyType(textboxPropertyType, memberTypeAlias);
                    saveRequired = true;
                }

                if (saveRequired)
                    memberTypeService.Save(memberType);

                var macroService = ApplicationContext.Current.Services.MacroService;
                const string macroAlias = "MembersResetPassword";
                if (macroService.GetByAlias(macroAlias) == null)
                {
                    // Run migration

                    var macro = new Macro
                    {
                        Name = "Members Reset Password",
                        Alias = macroAlias,
                        ScriptingFile = "~/Views/MacroPartials/Members/ResetPassword.cshtml",
                        UseInEditor = true
                    };
                    macro.Save();
                }

                var contentService = ApplicationContext.Current.Services.ContentService;
                var rootNode = contentService.GetRootContent().OrderBy(x => x.SortOrder).First(x => x.ContentType.Alias == "Community");
                var memberNode = rootNode.Children().FirstOrDefault(x => string.Equals(x.Name, "Member", StringComparison.InvariantCultureIgnoreCase));

                var resetPasswordPageName = "Reset Password";
                if (memberNode != null && memberNode.Children().Any(x => x.Name == resetPasswordPageName) == false)
                {
                    var content = contentService.CreateContent(resetPasswordPageName, memberNode.Id, "Textpage");
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

        private void AddPRTeamPage()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                const string templateName = "TeamUmbraco";
                const string contentItemName = "TeamUmbraco";
                var relativeTemplateLocation = $"~/Views/{templateName}.cshtml";

                var contentService = ApplicationContext.Current.Services.ContentService;
                var rootContent = contentService.GetRootContent().FirstOrDefault();

                if (rootContent != null)
                {
                    var teamUmbracoPage = rootContent.Children().FirstOrDefault(x => x.Name == contentItemName);
                    if (teamUmbracoPage != null)
                    {
                        var templateContents = string.Empty;

                        var templateFile = HostingEnvironment.MapPath(relativeTemplateLocation);
                        if (templateFile != null && File.Exists(templateFile))
                            templateContents = File.ReadAllText(templateFile);

                        var templateCreateResult =
                            ApplicationContext.Current.Services.FileService.CreateTemplateForContentType("TextPage",
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
                            var textPageContentType = contentTypeService.GetContentType("TextPage");
                            var allowedTemplates = new List<ITemplate> { template };
                            allowedTemplates.AddRange(textPageContentType.AllowedTemplates);
                            textPageContentType.AllowedTemplates = allowedTemplates;
                            contentTypeService.Save(textPageContentType);
                        }

                        var textPage = contentService.CreateContent(contentItemName, rootContent.Id, "TextPage");
                        textPage.SetValue("umbracoNaviHide", true);
                        textPage.Template = ApplicationContext.Current.Services.FileService.GetTemplate(templateName);
                        var saveResult = contentService.SaveAndPublishWithStatus(textPage);
                    }

                    string[] lines = { "" };
                    File.WriteAllLines(path, lines);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddReposDashboardPage()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                const string templateName = "Dashboard";
                const string contentItemName = "Dashboard";
                var relativeTemplateLocation = $"~/Views/{templateName}.cshtml";

                var contentService = ApplicationContext.Current.Services.ContentService;
                var rootContent = contentService.GetRootContent().FirstOrDefault();

                if (rootContent != null)
                {
                    var dashboardPage = rootContent.Children().FirstOrDefault(x => x.Name == contentItemName);
                    if (dashboardPage != null)
                    {
                        var templateContents = string.Empty;

                        var templateFile = HostingEnvironment.MapPath(relativeTemplateLocation);
                        if (templateFile != null && File.Exists(templateFile))
                            templateContents = File.ReadAllText(templateFile);

                        var templateCreateResult =
                            ApplicationContext.Current.Services.FileService.CreateTemplateForContentType("TextPage",
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
                            var textPageContentType = contentTypeService.GetContentType("TextPage");
                            var allowedTemplates = new List<ITemplate> { template };
                            allowedTemplates.AddRange(textPageContentType.AllowedTemplates);
                            textPageContentType.AllowedTemplates = allowedTemplates;
                            contentTypeService.Save(textPageContentType);
                        }

                        var textPage = contentService.CreateContent(contentItemName, rootContent.Id, "TextPage");
                        textPage.SetValue("umbracoNaviHide", true);
                        textPage.Template = ApplicationContext.Current.Services.FileService.GetTemplate(templateName);
                        var saveResult = contentService.SaveAndPublishWithStatus(textPage);
                    }

                    string[] lines = { "" };
                    File.WriteAllLines(path, lines);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddAppsDashboardPage()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                const string templateName = "Apps";
                const string contentItemName = "Apps";
                var relativeTemplateLocation = $"~/Views/{templateName}.cshtml";

                var contentService = ApplicationContext.Current.Services.ContentService;
                var rootContent = contentService.GetRootContent().FirstOrDefault();

                if (rootContent != null)
                {
                    var appsPage = rootContent.Children().FirstOrDefault(x => x.Name == contentItemName);
                    if (appsPage != null)
                    {
                        var templateContents = string.Empty;

                        var templateFile = HostingEnvironment.MapPath(relativeTemplateLocation);
                        if (templateFile != null && File.Exists(templateFile))
                            templateContents = File.ReadAllText(templateFile);

                        var templateCreateResult =
                            ApplicationContext.Current.Services.FileService.CreateTemplateForContentType("TextPage", templateName);
                        if (templateCreateResult.Success)
                        {
                            var template = ApplicationContext.Current.Services.FileService.GetTemplate(templateName);
                            var masterTemplate = ApplicationContext.Current.Services.FileService.GetTemplate("master");
                            template.SetMasterTemplate(masterTemplate);
                            if (templateContents != string.Empty)
                                template.Content = templateContents;
                            ApplicationContext.Current.Services.FileService.SaveTemplate(template);

                            var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                            var textPageContentType = contentTypeService.GetContentType("TextPage");
                            var allowedTemplates = new List<ITemplate> { template };
                            allowedTemplates.AddRange(textPageContentType.AllowedTemplates);
                            textPageContentType.AllowedTemplates = allowedTemplates;
                            contentTypeService.Save(textPageContentType);
                        }

                        var textPage = contentService.CreateContent(contentItemName, rootContent.Id, "TextPage");
                        textPage.SetValue("umbracoNaviHide", true);
                        textPage.Template = ApplicationContext.Current.Services.FileService.GetTemplate(templateName);
                        var saveResult = contentService.SaveAndPublishWithStatus(textPage);
                    }

                    string[] lines = { "" };
                    File.WriteAllLines(path, lines);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddGitHubMemberProperties()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;
            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                {
                    return;
                }
                // Get references to the needed services
                var dts = ApplicationContext.Current.Services.DataTypeService;
                var mts = ApplicationContext.Current.Services.MemberTypeService;
                // Attempt to find the "member" member type
                var memberType = mts.Get("member");
                if (memberType == null) throw new Exception("WTF? Unable to find member type with alias: member");
                bool hasIdProperty = memberType.PropertyTypes.Any(x => x.Alias == "githubId");
                bool hasDataProperty = memberType.PropertyTypes.Any(x => x.Alias == "githubData");
                bool hasChanges = false;
                if (!hasIdProperty)
                {
                    // Get the "Textstring" data type definition
                    var dtd = dts.GetDataTypeDefinitionById(-88);
                    // Initialize a new property type based on the data type
                    var pt = new PropertyType(dtd, "githubId") { Name = "GitHub ID", Description = "The ID of the linked GitHub user." };
                    // Add the property to the "Services" group/tab
                    memberType.AddPropertyType(pt, "Services");
                    // We do now
                    hasChanges = true;
                }
                if (!hasDataProperty)
                {
                    // Just a random, but hardcoded GUID
                    var dtdKey = new Guid("cd7b4e99-faff-46c6-affa-9c95178df336");
                    // "JSON Preview"
                    var dtd = dts.GetDataTypeDefinitionById(dtdKey);
                    // Create the data type difinition if not found
                    if (dtd == null)
                    {
                        dtd = new DataTypeDefinition(-1, "Our.JsonPreview");
                        dtd.Key = dtdKey;
                        dtd.Name = "GitHub user data";
                        dts.Save(dtd);
                    }
                    // Initialize a new property type based on the data type
                    var pt = new PropertyType(dtd, "githubData") { Name = "GitHub Data", Description = "The data of the linked GitHub user." };
                    // Add the property to the "Services" group/tab
                    memberType.AddPropertyType(pt, "Services");
                    // We do now
                    hasChanges = true;
                }
                // Save the member type if we have any changes for it
                if (hasChanges) mts.Save(memberType);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddTwitterMemberProperties()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;
            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                {
                    return;
                }
                // Get references to the needed services
                var dts = ApplicationContext.Current.Services.DataTypeService;
                var mts = ApplicationContext.Current.Services.MemberTypeService;
                // Attempt to find the "member" member type
                var memberType = mts.Get("member");
                if (memberType == null) throw new Exception("WTF? Unable to find member type with alias: member");
                bool hasIdProperty = memberType.PropertyTypes.Any(x => x.Alias == "twitterId");
                bool hasDataProperty = memberType.PropertyTypes.Any(x => x.Alias == "TwitterData");
                bool hasChanges = false;
                if (!hasIdProperty)
                {
                    // Get the "Textstring" data type definition
                    var dtd = dts.GetDataTypeDefinitionById(-88);
                    // Initialize a new property type based on the data type
                    var pt = new PropertyType(dtd, "twitterId") { Name = "Twitter ID", Description = "The ID of the linked Twitter user." };
                    // Add the property to the "Services" group/tab
                    memberType.AddPropertyType(pt, "Services");
                    // We do now
                    hasChanges = true;
                }
                if (!hasDataProperty)
                {
                    // Just a random, but hardcoded GUID
                    var dtdKey = new Guid("24a673ff-d198-4931-8112-67e20cb6e948");
                    // "JSON Preview"
                    var dtd = dts.GetDataTypeDefinitionById(dtdKey);
                    // Create the data type difinition if not found
                    if (dtd == null)
                    {
                        dtd = new DataTypeDefinition(-1, "Our.JsonPreview");
                        dtd.Key = dtdKey;
                        dtd.Name = "Twitter user data";
                        dts.Save(dtd);
                    }
                    // Initialize a new property type based on the data type
                    var pt = new PropertyType(dtd, "twitterData") { Name = "Twitter Data", Description = "The data of the linked Twitter user." };
                    // Add the property to the "Services" group/tab
                    memberType.AddPropertyType(pt, "Services");
                    // We do now
                    hasChanges = true;
                }
                // Save the member type if we have any changes for it
                if (hasChanges) mts.Save(memberType);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddAdditionalUsers()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var userService = ApplicationContext.Current.Services.UserService;

                var db = ApplicationContext.Current.DatabaseContext.Database;

                for (var i = 1; i <= 45; i++)
                {
                    var user = userService.GetUserById(i);
                    if (user == null)
                        db.Execute($"INSERT INTO umbracoUser (userName, userLogin, userEmail, userPassword) VALUES('test{i}@@test.com', 'test{i}@@test.com', 'test{i}@@test.com', 'abc123')");
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddManualProgressSliderToReleases()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                var dataTypeService = ApplicationContext.Current.Services.DataTypeService;
                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var releaseContentType = contentTypeService.GetContentType("Release");
                var propertyTypeAlias = "overrideYouTrackReleaseProgress";

                // create the slider data type
                var progressSliderDataTypeName = "Release Progress Slider";

                var progressSliderDataType = dataTypeService.GetDataTypeDefinitionByName(progressSliderDataTypeName);
                if (progressSliderDataType == null)
                {
                    progressSliderDataType = new DataTypeDefinition(-1, "Umbraco.Slider");
                    Dictionary<string, PreValue> preValues = new Dictionary<string, PreValue>();
                    preValues.Add("enableRange", new PreValue("0"));
                    preValues.Add("orientation", new PreValue("horizontal"));
                    preValues.Add("initVal1", new PreValue("0"));
                    preValues.Add("initVal2", new PreValue(""));
                    preValues.Add("minVal", new PreValue("0"));
                    preValues.Add("maxVal", new PreValue("100"));
                    preValues.Add("step", new PreValue("1"));
                    preValues.Add("precision", new PreValue(""));
                    preValues.Add("handle", new PreValue("round"));
                    preValues.Add("tooltip", new PreValue("always"));
                    preValues.Add("tooltipSplit", new PreValue("0"));
                    preValues.Add("tooltipFormat", new PreValue(""));
                    preValues.Add("tooltipPosition", new PreValue(""));
                    preValues.Add("reversed", new PreValue("0"));
                    preValues.Add("ticks", new PreValue(""));
                    preValues.Add("ticksPositions", new PreValue(""));
                    preValues.Add("ticksLabels", new PreValue(""));
                    preValues.Add("ticksSnapBounds", new PreValue(""));
                    progressSliderDataType.Name = progressSliderDataTypeName;
                    dataTypeService.SaveDataTypeAndPreValues(progressSliderDataType, preValues);
                }

                var tabName = "Content";
                if (releaseContentType.PropertyGroups.Contains(tabName) == false)
                    releaseContentType.AddPropertyGroup(tabName);

                if (releaseContentType.PropertyTypeExists(propertyTypeAlias) == false)
                {
                    var progressSlider = new PropertyType(progressSliderDataType, propertyTypeAlias);
                    progressSlider.Name = "Override Automated Release Progress";
                    releaseContentType.AddPropertyType(progressSlider, tabName);
                }

                contentTypeService.Save(releaseContentType);

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddSingleMediaPickerDataType()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path) == true)
                {
                    return;
                }

                var dataTypeService = ApplicationContext.Current.Services.DataTypeService;

                var alias = "Media Picker - Single image";
                var mediaPickerType = dataTypeService.GetDataTypeDefinitionByName(alias);
                if (mediaPickerType == null)
                {
                    var dataType = new DataTypeDefinition(-1, "Umbraco.MediaPicker2")
                    {
                        Name = alias
                    };

                    var preValues = new Dictionary<string, PreValue>
                    {
                        { "multiPicker", new PreValue("0") },
                        { "onlyImages", new PreValue("1") },
                        { "disableFolderSelect", new PreValue("1") }
                    };

                    dataTypeService.SaveDataTypeAndPreValues(dataType, preValues);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddLocationDataTypes()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path) == true)
                {
                    return;
                }

                var dataTypeService = ApplicationContext.Current.Services.DataTypeService;

                var continentAlias = "Locations - Continents";
                var continentType = dataTypeService.GetDataTypeDefinitionByName(continentAlias);
                if (continentType == null)
                {
                    var dataType = new DataTypeDefinition(-1, "Umbraco.DropDown.Flexible")
                    {
                        Name = continentAlias
                    };

                    var preValues = new Dictionary<string, PreValue>
                    {
                        { "0", new PreValue("Africa") },
                        { "1", new PreValue("Antarctica") },
                        { "2", new PreValue("Australia") },
                        { "3", new PreValue("Asia") },
                        { "4", new PreValue("Europe") },
                        { "5", new PreValue("North America") },
                        { "6", new PreValue("South America") },
                        { "multiple", new PreValue("1") }
                    };

                    dataTypeService.SaveDataTypeAndPreValues(dataType, preValues);
                }

                var countryAlias = "Locations - Contries";
                var countryType = dataTypeService.GetDataTypeDefinitionByName(countryAlias);
                if (countryType == null)
                {
                    var dataType = new DataTypeDefinition(-1, "Umbraco.DropDown.Flexible")
                    {
                        Name = countryAlias
                    };

                    #region Prevalues with countries
                    var preValues = new Dictionary<string, PreValue>
                    {
                        { "0", new PreValue("Afghanistan") },
                        { "1", new PreValue("Åland Islands") },
                        { "2", new PreValue("Albania") },
                        { "3", new PreValue("Algeria") },
                        { "4", new PreValue("American Samoa") },
                        { "5", new PreValue("Andorra") },
                        { "6", new PreValue("Angola") },
                        { "7", new PreValue("Anguilla") },
                        { "8", new PreValue("Antigua and Barbuda") },
                        { "9", new PreValue("Argentina") },
                        { "10", new PreValue("Armenia") },
                        { "11", new PreValue("Aruba") },
                        { "12", new PreValue("Australia") },
                        { "13", new PreValue("Austria") },
                        { "14", new PreValue("Azerbaijan") },
                        { "15", new PreValue("Bahamas") },
                        { "16", new PreValue("Bahrain") },
                        { "17", new PreValue("Bangladesh") },
                        { "18", new PreValue("Barbados") },
                        { "19", new PreValue("Belarus") },
                        { "20", new PreValue("Belgium") },
                        { "21", new PreValue("Belize") },
                        { "22", new PreValue("Benin") },
                        { "23", new PreValue("Bermuda") },
                        { "24", new PreValue("Bhutan") },
                        { "25", new PreValue("Bolivarian Republic of Venezuela") },
                        { "26", new PreValue("Bolivia") },
                        { "27", new PreValue("Bonaire, Sint Eustatius and Saba") },
                        { "28", new PreValue("Bosnia and Herzegovina") },
                        { "29", new PreValue("Botswana") },
                        { "30", new PreValue("Brazil") },
                        { "31", new PreValue("British Indian Ocean Territory") },
                        { "32", new PreValue("British Virgin Islands") },
                        { "33", new PreValue("Brunei Darussalam") },
                        { "34", new PreValue("Bulgaria") },
                        { "35", new PreValue("Burkina Faso") },
                        { "36", new PreValue("Burundi") },
                        { "37", new PreValue("Cabo Verde") },
                        { "38", new PreValue("Cambodia") },
                        { "39", new PreValue("Cameroon") },
                        { "40", new PreValue("Canada") },
                        { "41", new PreValue("Caribbean") },
                        { "42", new PreValue("Cayman Islands") },
                        { "43", new PreValue("Central African Republic") },
                        { "44", new PreValue("Chad") },
                        { "45", new PreValue("Chile") },
                        { "46", new PreValue("Christmas Island") },
                        { "47", new PreValue("Città del Vaticano") },
                        { "48", new PreValue("Cocos (Keeling) Islands") },
                        { "49", new PreValue("Colombia") },
                        { "50", new PreValue("Comoros") },
                        { "51", new PreValue("Congo") },
                        { "52", new PreValue("Congo (DRC)") },
                        { "53", new PreValue("Cook Islands") },
                        { "54", new PreValue("Costa Rica") },
                        { "55", new PreValue("Côte d’Ivoire") },
                        { "56", new PreValue("Croatia") },
                        { "57", new PreValue("Cuba") },
                        { "58", new PreValue("Curaçao") },
                        { "59", new PreValue("Cyprus") },
                        { "60", new PreValue("Czech Republic") },
                        { "61", new PreValue("Denmark") },
                        { "62", new PreValue("Djibouti") },
                        { "63", new PreValue("Dominica") },
                        { "64", new PreValue("Dominican Republic") },
                        { "65", new PreValue("Ecuador") },
                        { "66", new PreValue("Egypt") },
                        { "67", new PreValue("El Salvador") },
                        { "68", new PreValue("Equatorial Guinea") },
                        { "69", new PreValue("Eritrea") },
                        { "70", new PreValue("Estonia") },
                        { "71", new PreValue("Ethiopia") },
                        { "72", new PreValue("Europe") },
                        { "73", new PreValue("Falkland Islands") },
                        { "74", new PreValue("Faroe Islands") },
                        { "75", new PreValue("Fiji") },
                        { "76", new PreValue("Finland") },
                        { "77", new PreValue("France") },
                        { "78", new PreValue("French Guiana") },
                        { "79", new PreValue("French Polynesia") },
                        { "80", new PreValue("Gabon") },
                        { "81", new PreValue("Gambia") },
                        { "82", new PreValue("Georgia") },
                        { "83", new PreValue("Germany") },
                        { "84", new PreValue("Ghana") },
                        { "85", new PreValue("Gibraltar") },
                        { "86", new PreValue("Greece") },
                        { "87", new PreValue("Greenland") },
                        { "88", new PreValue("Grenada") },
                        { "89", new PreValue("Guadeloupe") },
                        { "90", new PreValue("Guam") },
                        { "91", new PreValue("Guatemala") },
                        { "92", new PreValue("Guernsey") },
                        { "93", new PreValue("Guinea") },
                        { "94", new PreValue("Guinea-Bissau") },
                        { "95", new PreValue("Guyana") },
                        { "96", new PreValue("Haiti") },
                        { "97", new PreValue("Honduras") },
                        { "98", new PreValue("Hong Kong S.A.R.") },
                        { "99", new PreValue("Hungary") },
                        { "100", new PreValue("Iceland") },
                        { "101", new PreValue("India") },
                        { "102", new PreValue("Indonesia") },
                        { "103", new PreValue("Iran") },
                        { "104", new PreValue("Iraq") },
                        { "105", new PreValue("Ireland") },
                        { "106", new PreValue("Islamic Republic of Pakistan") },
                        { "107", new PreValue("Isle of Man") },
                        { "108", new PreValue("Israel") },
                        { "109", new PreValue("Italy") },
                        { "110", new PreValue("Jamaica") },
                        { "111", new PreValue("Japan") },
                        { "112", new PreValue("Jersey") },
                        { "113", new PreValue("Jordan") },
                        { "114", new PreValue("Kazakhstan") },
                        { "115", new PreValue("Kenya") },
                        { "116", new PreValue("Kiribati") },
                        { "117", new PreValue("Korea") },
                        { "118", new PreValue("Kosovo") },
                        { "119", new PreValue("Kuwait") },
                        { "120", new PreValue("Kyrgyzstan") },
                        { "121", new PreValue("Lao P.D.R.") },
                        { "122", new PreValue("Latin America") },
                        { "123", new PreValue("Latvia") },
                        { "124", new PreValue("Lebanon") },
                        { "125", new PreValue("Lesotho") },
                        { "126", new PreValue("Liberia") },
                        { "127", new PreValue("Libya") },
                        { "128", new PreValue("Liechtenstein") },
                        { "129", new PreValue("Lithuania") },
                        { "130", new PreValue("Luxembourg") },
                        { "131", new PreValue("Macao S.A.R.") },
                        { "132", new PreValue("Macedonia (FYROM)") },
                        { "133", new PreValue("Madagascar") },
                        { "134", new PreValue("Malawi") },
                        { "135", new PreValue("Malaysia") },
                        { "136", new PreValue("Maldives") },
                        { "137", new PreValue("Mali") },
                        { "138", new PreValue("Malta") },
                        { "139", new PreValue("Marshall Islands") },
                        { "140", new PreValue("Martinique") },
                        { "141", new PreValue("Mauritania") },
                        { "142", new PreValue("Mauritius") },
                        { "143", new PreValue("Mayotte") },
                        { "144", new PreValue("Mexico") },
                        { "145", new PreValue("Micronesia") },
                        { "146", new PreValue("Moldova") },
                        { "147", new PreValue("Mongolia") },
                        { "148", new PreValue("Montenegro") },
                        { "149", new PreValue("Montserrat") },
                        { "150", new PreValue("Morocco") },
                        { "151", new PreValue("Mozambique") },
                        { "152", new PreValue("Myanmar") },
                        { "153", new PreValue("Namibia") },
                        { "154", new PreValue("Nauru") },
                        { "155", new PreValue("Nepal") },
                        { "156", new PreValue("Netherlands") },
                        { "157", new PreValue("New Caledonia") },
                        { "158", new PreValue("New Zealand") },
                        { "159", new PreValue("Nicaragua") },
                        { "160", new PreValue("Niger") },
                        { "161", new PreValue("Nigeria") },
                        { "162", new PreValue("Niue") },
                        { "163", new PreValue("Norfolk Island") },
                        { "164", new PreValue("Northern Mariana Islands") },
                        { "165", new PreValue("Norway") },
                        { "166", new PreValue("Oman") },
                        { "167", new PreValue("Palau") },
                        { "168", new PreValue("Palestinian Authority") },
                        { "169", new PreValue("Panama") },
                        { "170", new PreValue("Papua New Guinea") },
                        { "171", new PreValue("Paraguay") },
                        { "172", new PreValue("People's Republic of China") },
                        { "173", new PreValue("Peru") },
                        { "174", new PreValue("Philippines") },
                        { "175", new PreValue("Pitcairn Islands") },
                        { "176", new PreValue("Poland") },
                        { "177", new PreValue("Portugal") },
                        { "178", new PreValue("Principality of Monaco") },
                        { "179", new PreValue("Puerto Rico") },
                        { "180", new PreValue("Qatar") },
                        { "181", new PreValue("Réunion") },
                        { "182", new PreValue("Romania") },
                        { "183", new PreValue("Russia") },
                        { "184", new PreValue("Rwanda") },
                        { "185", new PreValue("Saint Barthélemy") },
                        { "186", new PreValue("Saint Kitts and Nevis") },
                        { "187", new PreValue("Saint Lucia") },
                        { "188", new PreValue("Saint Martin") },
                        { "189", new PreValue("Saint Pierre and Miquelon") },
                        { "190", new PreValue("Saint Vincent and the Grenadines") },
                        { "191", new PreValue("Samoa") },
                        { "192", new PreValue("San Marino") },
                        { "193", new PreValue("São Tomé and Príncipe") },
                        { "194", new PreValue("Saudi Arabia") },
                        { "195", new PreValue("Senegal") },
                        { "196", new PreValue("Serbia") },
                        { "197", new PreValue("Seychelles") },
                        { "198", new PreValue("Sierra Leone") },
                        { "199", new PreValue("Singapore") },
                        { "200", new PreValue("Sint Maarten") },
                        { "201", new PreValue("Slovakia") },
                        { "202", new PreValue("Slovenia") },
                        { "203", new PreValue("Solomon Islands") },
                        { "204", new PreValue("Somalia") },
                        { "205", new PreValue("South Africa") },
                        { "206", new PreValue("South Sudan") },
                        { "207", new PreValue("Spain") },
                        { "208", new PreValue("Sri Lanka") },
                        { "209", new PreValue("St Helena, Ascension, Tristan da Cunha") },
                        { "210", new PreValue("Sudan") },
                        { "211", new PreValue("Suriname") },
                        { "212", new PreValue("Svalbard and Jan Mayen") },
                        { "213", new PreValue("Swaziland") },
                        { "214", new PreValue("Sweden") },
                        { "215", new PreValue("Switzerland") },
                        { "216", new PreValue("Syria") },
                        { "217", new PreValue("Taiwan") },
                        { "218", new PreValue("Tajikistan") },
                        { "219", new PreValue("Tanzania") },
                        { "220", new PreValue("Thailand") },
                        { "221", new PreValue("Timor-Leste") },
                        { "222", new PreValue("Togo") },
                        { "223", new PreValue("Tokelau") },
                        { "224", new PreValue("Tonga") },
                        { "225", new PreValue("Trinidad and Tobago") },
                        { "226", new PreValue("Tunisia") },
                        { "227", new PreValue("Turkey") },
                        { "228", new PreValue("Turkmenistan") },
                        { "229", new PreValue("Turks and Caicos Islands") },
                        { "230", new PreValue("Tuvalu") },
                        { "231", new PreValue("U.A.E.") },
                        { "232", new PreValue("U.S. Outlying Islands") },
                        { "233", new PreValue("U.S. Virgin Islands") },
                        { "234", new PreValue("Uganda") },
                        { "235", new PreValue("Ukraine") },
                        { "236", new PreValue("United Kingdom") },
                        { "237", new PreValue("United States") },
                        { "238", new PreValue("Uruguay") },
                        { "239", new PreValue("Uzbekistan") },
                        { "240", new PreValue("Vanuatu") },
                        { "241", new PreValue("Vietnam") },
                        { "242", new PreValue("Wallis and Futuna") },
                        { "243", new PreValue("World") },
                        { "244", new PreValue("Yemen") },
                        { "245", new PreValue("Zambia") },
                        { "246", new PreValue("Zimbabwe") },
                        { "247", new PreValue("조선민주주의인민공화국") },
                        { "multiple", new PreValue("1") }
                    };
                    #endregion

                    dataTypeService.SaveDataTypeAndPreValues(dataType, preValues);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddBannerTypes()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path) == true)
                {
                    return;
                }

                var dataTypeService = ApplicationContext.Current.Services.DataTypeService;
                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;

                /// Single banner content type
                var bannerTypeAlias = "banner";
                var bannerType = contentTypeService.GetContentType(bannerTypeAlias);
                if (bannerType == null)
                {
                    var contentType = new ContentType(-1)
                    {
                        Name = "Banner",
                        Alias = bannerTypeAlias,
                    };

                    contentType.PropertyGroups.Add(new PropertyGroup { Name = "Banner" });

                    var media = dataTypeService.GetDataTypeDefinitionByName("Media Picker - Single image");
                    var mediaPicker = new PropertyType(media, "image")
                    {
                        Name = "Banner",
                        Description = "Select a media item to display.",
                        Mandatory = true
                    };
                    contentType.AddPropertyType(mediaPicker, "Banner");

                    var link = new DataTypeDefinition("Umbraco.Textbox");
                    var linkBox = new PropertyType(link, "link")
                    {
                        Name = "Link",
                        Description = "An external link to the festival.",
                        Mandatory = true
                    };
                    contentType.AddPropertyType(linkBox, "Banner");

                    var content = new DataTypeDefinition("Umbraco.TinyMCEv3");
                    var contentEditor = new PropertyType(content, "bodyText")
                    {
                        Name = "Content",
                        Description = "Text to appear under the banner.",
                        Mandatory = false
                    };
                    contentType.AddPropertyType(contentEditor, "Banner");

                    var check = new DataTypeDefinition("Umbraco.TrueFalse");
                    var checkBox = new PropertyType(check, "all")
                    {
                        Name = "Visible to all?",
                        Description = "This banner will be visible to all if the checkbox is checked."
                    };
                    contentType.AddPropertyType(checkBox, "Banner");

                    var continents = dataTypeService.GetDataTypeDefinitionByName("Locations - Continents");
                    var continentsDropdown = new PropertyType(continents, "continents")
                    {
                        Name = "Continents",
                        Description = "Select in which continents this banner should be displayed"
                    };
                    contentType.AddPropertyType(continentsDropdown, "Banner");

                    var countries = dataTypeService.GetDataTypeDefinitionByName("Locations - Contries");
                    var countriesDropdown = new PropertyType(countries, "countries")
                    {
                        Name = "Countries",
                        Description = "Select in which countries this banner should be displayed"
                    };
                    contentType.AddPropertyType(countriesDropdown, "Banner");

                    contentTypeService.Save(contentType);
                }

                // Banner container content type
                var bannersTypeAlias = "banners";
                var bannersType = contentTypeService.GetContentType(bannersTypeAlias);
                if (bannersType == null)
                {
                    var contentType = new ContentType(-1)
                    {
                        Name = "Banners",
                        Alias = bannersTypeAlias,
                        IsContainer = true,
                        AllowedAsRoot = true
                    };

                    var banner = contentTypeService.GetContentType("banner");
                    if (banner != null)
                    {
                        contentType.AllowedContentTypes = new List<ContentTypeSort> { new ContentTypeSort(banner.Id, 0) };
                    }

                    contentTypeService.Save(contentType);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddBannersPage()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path) == true)
                {
                    return;
                }

                var contentService = ApplicationContext.Current.Services.ContentService;
                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;

                var bannersType = contentTypeService.GetContentType("banners");
                if (bannersType != null)
                {
                    var content = contentService.CreateContent("Banners", -1, bannersType.Alias);
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

        private void AddBannersPicker()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path) == true)
                {
                    return;
                }

                var dataTypeService = ApplicationContext.Current.Services.DataTypeService;
                var contentService = ApplicationContext.Current.Services.ContentService;

                var alias = "Banners - Tree Picker";
                var treePickerType = dataTypeService.GetDataTypeDefinitionByName(alias);
                if (treePickerType == null)
                {
                    var dataType = new DataTypeDefinition(-1, "Umbraco.MultiNodeTreePicker2")
                    {
                        Name = alias
                    };

                    var preValues = new Dictionary<string, PreValue>
                    {
                        { "filter", new PreValue("banner") },
                        { "minNumber", new PreValue("0") },
                        { "maxNumber", new PreValue("15") },
                    };

                    dataTypeService.SaveDataTypeAndPreValues(dataType, preValues);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddBannersToCommunityPage()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path) == true)
                {
                    return;
                }

                var dataTypeService = ApplicationContext.Current.Services.DataTypeService;
                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;

                var communityContentType = contentTypeService.GetContentType("community");
                if (communityContentType != null && communityContentType.PropertyTypeExists("banners") == false)
                {
                    var picker = dataTypeService.GetDataTypeDefinitionByName("Banners - Tree Picker");
                    var pickerPropertyType = new PropertyType(picker, "banners") { Name = "Banners", Description = "Select banners to display." };
                    communityContentType.AddPropertyType(pickerPropertyType, "Banners");

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

        private void RemoveHomeOnlyBannerTextArea()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path) == true)
                {
                    return;
                }

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;

                var communityContentType = contentTypeService.GetContentType("Community");
                var propertyTypeAlias = "homeOnlyBanner";
                if (communityContentType.PropertyTypeExists(propertyTypeAlias) == true)
                {
                    communityContentType.RemovePropertyType("homeOnlyBanner");
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

        private void CreateGitHubAutoRepliesTable()
        {

            var migrationName = MethodBase.GetCurrentMethod().Name;

            var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
            if (File.Exists(path)) return;

            try
            {

                var schema = new DatabaseSchemaHelper(
                    ApplicationContext.Current.DatabaseContext.Database,
                    ApplicationContext.Current.ProfilingLogger.Logger,
                    ApplicationContext.Current.DatabaseContext.SqlSyntax
                );

                if (schema.TableExist<GitHubAutoReplyPoco>() == false)
                {
                    schema.CreateTable<GitHubAutoReplyPoco>();
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);

            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }

        }

        private void AddMapToggleToCommunityHub()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path)) return;

                var dataTypeService = ApplicationContext.Current.Services.DataTypeService;
                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var communityHubContentType = contentTypeService.GetContentType("communityHub");
                if (communityHubContentType != null && !communityHubContentType.PropertyTypeExists("showMap"))
                {
                    var rte = dataTypeService.GetDataTypeDefinitionByName("Checkbox");
                    var pickerPropertyType = new PropertyType(rte, "showMap")
                    {
                        Name = "Show map?",
                        Description = "Renderes the community member map if toggled."
                    };
                    communityHubContentType.AddPropertyType(pickerPropertyType, "Content");

                    contentTypeService.Save(communityHubContentType);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);

            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddSmallRteDataType()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path)) return;

                var dataTypeService = ApplicationContext.Current.Services.DataTypeService;

                var alias = "Richtext Editor - Small";
                var mediaPickerType = dataTypeService.GetDataTypeDefinitionByName(alias);
                if (mediaPickerType == null)
                {
                    var dataType = new DataTypeDefinition(-1, "Umbraco.TinyMCEv3")
                    {
                        Name = alias
                    };

                    dataTypeService.SaveDataTypeAndPreValues(dataType, new Dictionary<string, PreValue> ());
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);

            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddNotificationToCommunityHub()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path)) return;

                var dataTypeService = ApplicationContext.Current.Services.DataTypeService;
                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var communityHubContentType = contentTypeService.GetContentType("communityHub");
                if (communityHubContentType != null && !communityHubContentType.PropertyTypeExists("notification"))
                {
                    var rte = dataTypeService.GetDataTypeDefinitionByName("Richtext Editor - Small");
                    var pickerPropertyType = new PropertyType(rte, "notification")
                    {
                        Name = "Notification",
                        Description = "This notification will be displayed at the top of the community hub page."
                    };
                    communityHubContentType.AddPropertyType(pickerPropertyType, "Content");

                    contentTypeService.Save(communityHubContentType);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);

            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddRtesToCommunityHub()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path)) return;

                var dataTypeService = ApplicationContext.Current.Services.DataTypeService;
                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var communityHubContentType = contentTypeService.GetContentType("communityHub");
                if (communityHubContentType != null)
                {
                    // Primary (above the map)
                    if (!communityHubContentType.PropertyTypeExists("primaryRte"))
                    {
                        var rte = dataTypeService.GetDataTypeDefinitionByName("Richtext Editor");
                        var pickerPropertyType = new PropertyType(rte, "primaryRte") { Name = "Content", Description = "Content to have displayed above the map." };
                        communityHubContentType.AddPropertyType(pickerPropertyType, "Content");

                        contentTypeService.Save(communityHubContentType);
                    }

                    // Secondary (below the map)
                    if (!communityHubContentType.PropertyTypeExists("secondaryRte"))
                    {
                        var rte = dataTypeService.GetDataTypeDefinitionByName("Richtext Editor");
                        var pickerPropertyType = new PropertyType(rte, "secondaryRte") { Name = "Content", Description = "Content to have displayed below the map." };
                        communityHubContentType.AddPropertyType(pickerPropertyType, "Content");

                        contentTypeService.Save(communityHubContentType);
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

        private void AddCommunityBlogPostsTemplate()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path)) return;

                var hubPageAlias = "communityHubPage";
                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var fileService = ApplicationContext.Current.Services.FileService;

                var templateName = "CommunityHubUProfileBlogPosts";

                var contentType = contentTypeService.GetContentType(hubPageAlias);
                if (contentType != null)
                {

                    var relativeTemplateLocation = $"~/Views/{templateName}.cshtml";

                    var templateContents = string.Empty;

                    var templateFile = HostingEnvironment.MapPath(relativeTemplateLocation);
                    if (templateFile != null && File.Exists(templateFile))
                        templateContents = File.ReadAllText(templateFile);

                    var template = fileService.CreateTemplateWithIdentity(templateName, templateContents);

                    var allowsTemplates = contentType.AllowedTemplates.ToList();
                    allowsTemplates.Add(template);
                    contentType.AllowedTemplates = allowsTemplates;
                    contentTypeService.Save(contentType);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);

            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddPackageLandingPageTemplate()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path))
                    return;

                const string templateName = "ProjectLanding";
                const string contentTypeAlias = "TextPage";
                var relativeTemplateLocation = $"~/Views/{templateName}.cshtml";

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var fileService = ApplicationContext.Current.Services.FileService;

                var contentType = contentTypeService.GetContentType(contentTypeAlias);
                if (contentType != null)
                {
                    var templateContents = string.Empty;

                    var templateFile = HostingEnvironment.MapPath(relativeTemplateLocation);
                    if (templateFile != null && File.Exists(templateFile))
                        templateContents = File.ReadAllText(templateFile);

                    var template = fileService.CreateTemplateWithIdentity(templateName, templateContents);

                    var allowsTemplates = contentType.AllowedTemplates.ToList();
                    allowsTemplates.Add(template);
                    contentType.AllowedTemplates = allowsTemplates;
                    contentTypeService.Save(contentType);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>(string.Format("Migration: '{0}' failed", migrationName), ex);
            }
        }

        private void AddEnhancedTextPage()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path)) return;

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;

                var name = "Enhanced Text Page";
                var alias = "enhancedTextPage";

                var enhancedTextPageType = contentTypeService.GetContentType(alias);
                if (enhancedTextPageType == null)
                {
                    var contentType = new ContentType(-1)
                    {
                        Name = name,
                        Alias = alias,
                    };

                    contentTypeService.Save(contentType);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>($"Migration: {migrationName} failed", ex);
            }
        }

        private void AddEnhancedTextPageTemplate()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path)) return;

                const string templateName = "EnhancedTextPage";
                const string contentTypeAlias = "enhancedTextPage";
                var relativeTemplateLocation = $"~/Views/{templateName}.cshtml";

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var fileService = ApplicationContext.Current.Services.FileService;

                var contentType = contentTypeService.GetContentType(contentTypeAlias);
                if (contentType != null)
                {
                    var templateContents = string.Empty;

                    var templateFile = HostingEnvironment.MapPath(relativeTemplateLocation);
                    if (templateFile != null && File.Exists(templateFile))
                        templateContents = File.ReadAllText(templateFile);

                    var template = fileService.CreateTemplateWithIdentity(templateName, templateContents);

                    var allowsTemplates = contentType.AllowedTemplates.ToList();
                    allowsTemplates.Add(template);
                    contentType.AllowedTemplates = allowsTemplates;
                    contentTypeService.Save(contentType);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>($"Migration: {migrationName} failed", ex);
            }
        }

        private void AddGridDataType()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path)) return;

                var dataTypeService = ApplicationContext.Current.Services.DataTypeService;

                var dataTypeAlias = "Grid Layout";

                var gridDataType = dataTypeService.GetDataTypeDefinitionByName(dataTypeAlias);
                if (gridDataType == null)
                {
                    var dataType = new DataTypeDefinition(-1, "Umbraco.Grid")
                    {
                        Name = dataTypeAlias
                    };

                    dataTypeService.Save(dataType);
                }

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>($"Migration: {migrationName} failed", ex);
            }
        }

        private void AddGridToEnhancedTextPage()
        {
            var migrationName = MethodBase.GetCurrentMethod().Name;

            try
            {
                var path = HostingEnvironment.MapPath(MigrationMarkersPath + migrationName + ".txt");
                if (File.Exists(path)) return;

                var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
                var dataTypeService = ApplicationContext.Current.Services.DataTypeService;

                var contentType = contentTypeService.GetContentType("enhancedTextPage");
                if (contentType != null)
                {
                    var name = "Content";

                    contentType.PropertyGroups.Add(new PropertyGroup { Name = name });

                    var grid = dataTypeService.GetDataTypeDefinitionByName("Grid Layout");
                    if (grid != null)
                    {
                        var gridProperty = new PropertyType(grid, "gridContent")
                        {
                            Name = "Content",
                            Description = "Content for the page",
                            Mandatory = false
                        };

                        contentType.AddPropertyType(gridProperty, name);
                    }

                    contentTypeService.Save(contentType);
                } 

                string[] lines = { "" };
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MigrationsHandler>($"Migration: {migrationName} failed", ex);
            }
        }
    }
}