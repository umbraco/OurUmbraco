using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using umbraco.cms.businesslogic.macro;
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
    }
}
