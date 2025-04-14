using Hangfire;
using Hangfire.Server;
using OurUmbraco.Community.GitHub;
using OurUmbraco.Community.BlogPosts;
using OurUmbraco.Community.Mastodon;
using OurUmbraco.Community.Meetup;
using OurUmbraco.Documentation;
using OurUmbraco.Our.Services;

namespace OurUmbraco.NotificationsCore.Notifications
{
    public class ScheduleHangfireJobs
    {
        public void CacheUpcomingMeetups(PerformContext context)
        {
            var meetupService = new MeetupService();
            RecurringJob.AddOrUpdate(() => meetupService.CacheUpcomingMeetups(), Cron.MinuteInterval(30));
        }

        public void UpdateCommunityBlogPosts(PerformContext context)
        {
            var service = new BlogPostsService();
            RecurringJob.AddOrUpdate(() => service.UpdateBlogPostsJsonFile(context), Cron.HourInterval(1));
        }

        public void GetGitHubPullRequests()
        {
            RecurringJob.AddOrUpdate(() => UpdatePullRequests(), Cron.HourInterval(1));
        }

        public void UpdatePullRequests()
        {
            var service = new GitHubService();
            service.UpdateAllPullRequestsForRepository();
        }

        public void GenerateReleasesCache(PerformContext context)
        {
            var releasesService = new ReleasesService();
            RecurringJob.AddOrUpdate(() => releasesService.GenerateReleasesCache(context), Cron.HourInterval(1));
        }

        public void UpdateGitHubIssues(PerformContext context)
        {
            var gitHubService = new GitHubService();
            var repository = new Community.Models.Repository("Umbraco-CMS", "umbraco", "Umbraco-CMS", "Umbraco-CMS");
            RecurringJob.AddOrUpdate($"[IssueTracker] Update {repository.Name}", () => gitHubService.UpdateIssues(context, repository), Cron.MinuteInterval(15));
        }
        
        public void FetchStaticApiDocumentation(PerformContext context)
        {
            var staticApiDocumentationService = new StaticApiDocumentationService();
            RecurringJob.AddOrUpdate(() => staticApiDocumentationService.FetchNewApiDocs(context), Cron.MinuteInterval(5));
        }
        public void FetchMastodonPosts(PerformContext context)
        {
            var mastodonService = new MastodonService();
            RecurringJob.AddOrUpdate(() => mastodonService.GetStatuses(10), Cron.MinuteInterval(2));
        }
    }
}