using Hangfire;
using Hangfire.Server;
using OurUmbraco.Community.GitHub;
using OurUmbraco.Community.BlogPosts;
using OurUmbraco.Community.Mastodon;
using OurUmbraco.Documentation;
using OurUmbraco.Our.Services;
using OurUmbraco.Community.Videos;
using OurUmbraco.Videos;

namespace OurUmbraco.NotificationsCore.Notifications
{
    public class ScheduleHangfireJobs
    {
        public void UpdateCommunityBlogPosts(PerformContext context)
        {
            var service = new BlogPostsService();
            RecurringJob.AddOrUpdate<BlogPostsService>("📝 Community blog posts", x => x.UpdateBlogPostsJsonFile(context), Cron.HourInterval(1));
        }

        public void GetGitHubPullRequests()
        {
            RecurringJob.AddOrUpdate<ScheduleHangfireJobs>("🚀 Get Umbraco-CMS PRs", x => x.UpdatePullRequests(), Cron.HourInterval(1));
        }

        public void UpdatePullRequests()
        {
            var service = new GitHubService();
            service.UpdateAllPullRequestsForRepository();
        }

        public void GenerateReleasesCache(PerformContext context)
        {
            var releasesService = new ReleasesService();
            RecurringJob.AddOrUpdate<ReleasesService>("💎 Release cache", x => x.GenerateReleasesCache(context), Cron.HourInterval(1));
        }

        public void UpdateGitHubIssues(PerformContext context)
        {
            var gitHubService = new GitHubService();
            var repository = new Community.Models.Repository("Umbraco-CMS", "umbraco", "Umbraco-CMS", "Umbraco-CMS");
            RecurringJob.AddOrUpdate($"📝 Update issues {repository.Name}", () => gitHubService.UpdateIssues(context, repository), Cron.MinuteInterval(15));
        }
        
        public void FetchStaticApiDocumentation(PerformContext context)
        {
            var staticApiDocumentationService = new StaticApiDocumentationService();
            RecurringJob.AddOrUpdate<StaticApiDocumentationService>("📝 Docs", x => x.FetchNewApiDocs(context), Cron.MinuteInterval(5));
        }
        public void FetchMastodonPosts(PerformContext context)
        {
            var mastodonService = new MastodonService();
            RecurringJob.AddOrUpdate<MastodonService>("️🐘 Get Masto posts", x => x.GetStatuses(10), Cron.MinuteInterval(2));
        }

        public void UpdateVimeoVideos()
        {   
            RecurringJob.AddOrUpdate<VideosService>("📽️ Update Vimeo videos", x => x.UpdateVimeoVideos("umbraco"), Cron.HourInterval(1));
        }

        public void UpdateCommunityVideos()
        {
            RecurringJob.AddOrUpdate<CommunityVideosService>("📽️ Update community videos", x => x.UpdateYouTubePlaylistVideos(), Cron.HourInterval(1));
        }
    }
}