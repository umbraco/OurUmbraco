using System;
using System.Text;
using System.Web.Hosting;
using Hangfire;
using Newtonsoft.Json;
using OurUmbraco.Community.GitHub;
using OurUmbraco.Community.BlogPosts;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace OurUmbraco.NotificationsCore.Notifications
{
    public class ScheduleHangfireJobs
    {
        public void UpdateCommunityBlogPosts()
        {
            RecurringJob.AddOrUpdate(() => UpdateBlogPostsJsonFile(), Cron.HourInterval(1));
        }

        public void UpdateBlogPostsJsonFile()
        {
            // Initialize a new service
            var service = new BlogPostsService();

            // Determine the path to the JSON file
            var jsonPath = HostingEnvironment.MapPath("~/App_Data/TEMP/CommunityBlogPosts.json");

            // Generate the raw JSON
            var rawJson = JsonConvert.SerializeObject(service.GetBlogPosts(), Formatting.Indented);

            // Save the JSON to disk
            System.IO.File.WriteAllText(jsonPath, rawJson, Encoding.UTF8);
        }

        public void MarkAsSolvedReminder()
        {
            RecurringJob.AddOrUpdate(() => ScheduleTopics(), Cron.HourInterval(12));
        }

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
    }

    public class ReminderTopic
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
    }
}