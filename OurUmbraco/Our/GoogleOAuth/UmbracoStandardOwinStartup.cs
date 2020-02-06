using System;
using System.Configuration;
using System.Web.Configuration;
using Hangfire;
using Hangfire.Console;
using Hangfire.SqlServer;
using Microsoft.Owin;
using OurUmbraco.Community.Map;
using OurUmbraco.NotificationsCore.Notifications;
using OurUmbraco.Our.GoogleOAuth;
using Owin;
using Umbraco.Web;

[assembly: OwinStartup("UmbracoStandardOwinStartup", typeof(UmbracoStandardOwinStartup))]
namespace OurUmbraco.Our.GoogleOAuth
{
    /// <summary>
    /// The standard way to configure OWIN for Umbraco
    /// </summary>
    /// <remarks>
    /// The startup type is specified in appSettings under owin:appStartup - change it to "StandardUmbracoStartup" to use this class
    /// </remarks>
    public class UmbracoStandardOwinStartup : UmbracoDefaultOwinStartup
    {
        public override void Configuration(IAppBuilder app)
        {
            //ensure the default options are configured
            base.Configuration(app);

            var clientId = WebConfigurationManager.AppSettings["GoogleOAuthClientID"];
            var secret = WebConfigurationManager.AppSettings["GoogleOAuthSecret"];
            app.ConfigureBackOfficeGoogleAuth(clientId, secret);

            app.MapSignalR();

            if (string.Equals(ConfigurationManager.AppSettings["HangFireEnabled"], "true", StringComparison.InvariantCultureIgnoreCase) == false)
                return;

            // Configure hangfire
            var options = new SqlServerStorageOptions { PrepareSchemaIfNecessary = true };
            var connectionString = Umbraco.Core.ApplicationContext.Current.DatabaseContext.ConnectionString;
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(connectionString, options)
                .UseConsole();

        
            var dashboardOptions = new DashboardOptions { Authorization = new[] { new UmbracoAuthorizationFilter() } };
            app.UseHangfireDashboard("/hangfire", dashboardOptions);
            app.UseHangfireServer();
            
            // Schedule jobs
            var scheduler = new ScheduleHangfireJobs();
            scheduler.MarkAsSolvedReminder();
            scheduler.UpdateGitHubContributors();
            scheduler.UpdateMeetupStats();
            scheduler.UpdateCommunityBlogPosts();
            scheduler.UpdateCommunityVideos();
            scheduler.UpdateVimeoVideos();
            scheduler.GetGitHubPullRequests();
            scheduler.RefreshKarmaStatistics();
            scheduler.GenerateReleasesCache(null);
            scheduler.UpdateGitHubIssues(null);
            scheduler.UpdateAllIssues(null);
            scheduler.GetAllGitHubLabels(null);
            scheduler.AddCommentToAwaitingFeedbackIssues(null);
            scheduler.AddCommentToUpForGrabsIssues(null);
            scheduler.NotifyUnmergeablePullRequests(null);
            scheduler.AddCommentToStateHQDiscussionIssues(null);

            scheduler.CheckContributorBadge(null);
            scheduler.GetNugetDownloads(null);
        }
    }
}