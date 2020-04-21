using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Examine;
using Hangfire;
using Hangfire.Server;
using Newtonsoft.Json;
using OurUmbraco.Community.GitHub;
using OurUmbraco.Community.BlogPosts;
using OurUmbraco.Community.Karma;
using OurUmbraco.Community.Videos;
using OurUmbraco.Our.Services;
using OurUmbraco.Videos;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace OurUmbraco.NotificationsCore.Notifications
{
    using OurUmbraco.Community.Nuget;

    public class ScheduleHangfireJobs
    {
        public void ScheduleTopics()
        {
            using (var db = ApplicationContext.Current.DatabaseContext.Database)
            {
                var sql = new Sql("SELECT id, memberId FROM forumTopics WHERE answer = 0 AND (markAsSolutionReminderSent IS NULL OR markAsSolutionReminderSent = 0) AND replies > 0 AND updated < getdate() - 7 AND created > '2016-10-01 00:00:00' AND id NOT IN (SELECT topicId FROM notificationMarkAsSolution) ORDER BY created DESC");
                var results = db.Query<ReminderTopic>(sql);
                var reminder = new MarkAsSolutionReminder();
                var notification = new NotificationMail();
                var reminderMail = notification.GetNotificationMail("MarkAsSolutionReminderSingle");

                foreach (var reminderTopic in results)
                {
                    var jobId = BackgroundJob.Schedule(() => reminder.SendNotification(reminderTopic.Id, reminderTopic.MemberId, reminderMail), TimeSpan.FromMinutes(10));
                }
            }
        }

        public void MarkAsSolvedReminder()
        {
            RecurringJob.AddOrUpdate(() => ScheduleTopics(), Cron.HourInterval(12));
        }

        public void UpdateGitHubContributors()
        {
            RecurringJob.AddOrUpdate(() => UpdateGitHubContributorsJsonFile(), Cron.HourInterval(12));
        }

        public void UpdateMeetupStats()
        {
            RecurringJob.AddOrUpdate(() => UpdateMeetupStatsJsonFile(), Cron.HourInterval(2));
        }

        public void UpdateGitHubContributorsJsonFile()
        {
            var service = new GitHubService();
            service.UpdateOverallContributors();
        }

        public void UpdateMeetupStatsJsonFile()
        {
            var service = new Community.Meetup.MeetupService();
            service.UpdateMeetupStats();
        }

        public void UpdateCommunityBlogPosts()
        {
            RecurringJob.AddOrUpdate(() => UpdateBlogPostsJsonFile(null), Cron.HourInterval(1));
        }

        public void UpdateVimeoVideos()
        {
            RecurringJob.AddOrUpdate(() => UpdateVimeoJsonFile(), Cron.HourInterval(1));
        }

        public void UpdateVimeoJsonFile()
        {
            var vimeoVideoService = new VideosService();
            vimeoVideoService.UpdateVimeoVideos("umbraco");
        }

        public void UpdateBlogPostsJsonFile(PerformContext context)
        {
            // Initialize a new service
            var service = new BlogPostsService();

            // Determine the path to the JSON file
            var jsonPath = HostingEnvironment.MapPath("~/App_Data/TEMP/CommunityBlogPosts.json");

            // Generate the raw JSON
            var rawJson = JsonConvert.SerializeObject(service.GetBlogPosts(context), Formatting.Indented);

            // Save the JSON to disk
            System.IO.File.WriteAllText(jsonPath, rawJson, Encoding.UTF8);
        }


        public void UpdateCommunityVideos()
        {
            RecurringJob.AddOrUpdate(() => UpdateCommunityVideosOnDisk(), Cron.HourInterval(1));
        }

        public void UpdateCommunityVideosOnDisk()
        {
            // Initialize a new service
            var service = new CommunityVideosService();
            service.UpdateYouTubePlaylistVideos();
        }

        public void GetGitHubPullRequests()
        {
            RecurringJob.AddOrUpdate(() => UpdatePullRequests(), Cron.HourInterval(1));
        }

        public void UpdatePullRequests()
        {
            var service = new GitHubService();
            service.UpdateAllPullRequestsForRepository();
            ExamineManager.Instance.IndexProviderCollection["PullRequestIndexer"].RebuildIndex();
        }

        public void RefreshKarmaStatistics()
        {
            var karmaService = new KarmaService();
            RecurringJob.AddOrUpdate(() => karmaService.RefreshKarmaStatistics(), Cron.MinuteInterval(10));
        }

        public void GenerateReleasesCache(PerformContext context)
        {
            var releasesService = new ReleasesService();
            RecurringJob.AddOrUpdate(() => releasesService.GenerateReleasesCache(context), Cron.HourInterval(1));
        }

        public void UpdateGitHubIssues(PerformContext context)
        {
            var repoManagementService = new RepositoryManagementService();
            var repositories = repoManagementService.GetAllPublicRepositories();

            var gitHubService = new GitHubService();
            foreach (var repository in repositories)
            {
                RecurringJob.AddOrUpdate($"[IssueTracker] Update {repository.Name}", () => gitHubService.UpdateIssues(context, repository), Cron.MinuteInterval(5));
            }
        }

        public void UpdateAllIssues(PerformContext context)
        {
            var repoManagementService = new RepositoryManagementService();
            var repositories = repoManagementService.GetAllPublicRepositories();

            var gitHubService = new GitHubService();
            foreach (var repository in repositories)
            {
                //RecurringJob.AddOrUpdate($"[IssueTracker] FullUpdate {repository.Name}", () => gitHubService.UpdateReviews(context, repository), Cron.Yearly(2, 31));
            }
        }

        public void GetAllGitHubLabels(PerformContext context)
        {
            var gitHubService = new GitHubService();
            RecurringJob.AddOrUpdate(() => gitHubService.DownloadAllLabels(context), Cron.MonthInterval(12));
        }

        public void AddCommentToUpForGrabsIssues(PerformContext context)
        {
            var gitHubService = new GitHubService();
            RecurringJob.AddOrUpdate(() => gitHubService.AddCommentToUpForGrabsIssues(context), Cron.MinuteInterval(10));
        }

        public void AddCommentToAwaitingFeedbackIssues(PerformContext context)
        {
            var gitHubService = new GitHubService();
            RecurringJob.AddOrUpdate(() => gitHubService.AddCommentToAwaitingFeedbackIssues(context), Cron.MinuteInterval(10));
        }

        public void AddCommentToStateHQDiscussionIssues(PerformContext context)
        {
            var gitHubService = new GitHubService();
            RecurringJob.AddOrUpdate(() => gitHubService.AddCommentToStateHQDiscussionIssues(context), Cron.MinuteInterval(10));
        }

        public void NotifyUnmergeablePullRequests(PerformContext context)
        {
            var gitHubService = new GitHubService();
            RecurringJob.AddOrUpdate(() => gitHubService.NotifyUnmergeablePullRequests(context), Cron.MonthInterval(12));
        }

        public void CheckContributorBadge(PerformContext context)
        {
            var contributors = new ContributorBadgeService();
            RecurringJob.AddOrUpdate(() => contributors.CheckContributorBadges(context), Cron.MinuteInterval(5));
        }

        public void GetNugetDownloads(PerformContext content)
        {
            var nugetService = new NugetPackageDownloadService();

            RecurringJob.AddOrUpdate(() => nugetService.ImportNugetPackageDownloads(), Cron.Daily);
        }

    }

    public class ReminderTopic
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
    }
}