﻿using System;
using System.Text;
using System.Web.Hosting;
using Examine;
using Hangfire;
using Hangfire.Server;
using Newtonsoft.Json;
using OurUmbraco.Community.GitHub;
using OurUmbraco.Community.BlogPosts;
using OurUmbraco.Community.Karma;
using OurUmbraco.Community.Videos;
using OurUmbraco.Videos;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace OurUmbraco.NotificationsCore.Notifications
{
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
            RecurringJob.AddOrUpdate(() => UpdateMeetupStatsJsonFile(), Cron.MinuteInterval(15));
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

            // Update the local storage of blog posts
            service.UpdateBlogPosts(context);

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
        
    }

    public class ReminderTopic
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
    }
}