using System;
using System.Linq;
using System.Text;
using System.Web.Security;
using Hangfire.Console;
using Hangfire.Server;
using OurUmbraco.Community.GitHub;
using OurUmbraco.Our.Models.GitHub;
using OurUmbraco.Our.Models.GitHub.AutoReplies;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace OurUmbraco.Our.Services
{

    /// <summary>
    /// Service class for working with GitHub issues, Our Umbraco members and contributor badges.
    /// </summary>
    public class ContributorBadgeService
    {

        private readonly RepositoryManagementService _repositories;

        private readonly GitHubService _github;

        /// <summary>
        /// Indicates whether live mode is activated. If <c>false</c>, the service will not post comments to GitHub.
        /// </summary>
        public bool IsLive = false;

        public ContributorBadgeService()
        {
            _repositories = new RepositoryManagementService();
            _github = new GitHubService();
        }

        /// <summary>
        /// Checks all PRs updated within the last ten minutes and adds the contributors to authors of merged PRs.
        /// </summary>
        /// <param name="hangfire">A reference to the Hangfire context if running from Hangfire.</param>
        public void CheckContributorBadges(PerformContext hangfire = null)
        {
            CheckContributorBadges(DateTime.UtcNow.Subtract(TimeSpan.FromDays(10)), hangfire);
        }

        /// <summary>
        /// Checks all PRs updated after <paramref name="since"/> and adds the contributors to authors of merged PRs.
        /// </summary>
        /// <param name="since">Only PRs since this timestamp will be checked.</param>
        /// <param name="hangfire">A reference to the Hangfire context if running from Hangfire.</param>
        public void CheckContributorBadges(DateTime since, PerformContext hangfire = null)
        {

            // Get a list of all the PRs
            var prs = _repositories.GetAllCommunityIssues(true, since);

            hangfire.WriteLine($"Found {prs.Count} PRs updated since " + since);

            foreach (var pr in prs)
            {
                CheckContributorBadges(pr, hangfire);
            }

            hangfire.WriteLine("Done. Yay :D");

        }

        /// <summary>
        /// Checks that the creator of <see cref="pr"/> has received the contributor badge.
        /// </summary>
        /// <param name="pr">The PR.</param>
        /// <param name="hangfire">A reference to the Hangfire context if running from Hangfire.</param>
        public void CheckContributorBadges(Issue pr, PerformContext hangfire = null)
        {

            if (IsLive == false)
                return;

            string prefix = $"[{pr.RepoSlug}/{pr.Number}]";

            // Look for a "merged" event in the list of events
            var merged = pr.Events.FirstOrDefault(x => x.Name == "merged");

            // We don't care about PR that hasn't been merged yet
            if (merged == null)
            {
                hangfire.WriteLine($"{prefix} PR has not been merged yet.");
                return;
            }

            // Return now if we've already sent the reply
            if (GitHubAutoReply.HasReply(pr, GitHubAutoReplyType.ContributorsBadgeAdded))
            {
                hangfire.WriteLine($"{prefix} Auto reply to {pr.User.Login} has already been posted.");
                return;
            }
            
            // Get the first member matching the GitHub user ID (or null if not found)
            var creator = _github.GetMemberByGitHubUserId(pr.User.Id);

            // Member wasn't found in Umbraco
            if (creator == null)
            {

                // Return now if we've already sent the reply
                if (GitHubAutoReply.HasReply(pr, GitHubAutoReplyType.ContributorsBadgeMemberNotFound))
                {
                    hangfire.WriteLine($"{prefix} No corresponding member found for creator {pr.User.Login} (ID: {pr.User.Id})");
                    return;
                }

                // Add the comment to the issue on GitHub
                if (IsLive) _github.AddCommentToIssue(pr, GitHubAutoReplyType.ContributorsBadgeMemberNotFound, GetContributorBadgeMemberNotFoundMessage(pr));

                return;
            }

            // Return if the member is already part of the role (aka has the contrib badge)
            if (Roles.IsUserInRole(creator.Email, Constants.MemberGroups.Contributor))
            {
                hangfire.WriteLine($"{prefix} Creator {pr.User.Login} already has the contributors badge");
                return;
            }

            // Add the member to the contrib role
            Roles.AddUserToRole(creator.Email, Constants.MemberGroups.Contributor);

            // Add the comment to the issue on GitHub
            if (IsLive) _github.AddCommentToIssue(pr, GitHubAutoReplyType.ContributorsBadgeAdded, GetContributorBadgeAddedMessage(pr, creator));

            hangfire.WriteLine($"{prefix} Creator {pr.User.Login} has successfully been given the contributors badge");

        }

        private string GetContributorBadgeMemberNotFoundMessage(Issue issue)
        {

            var ourUrl = "https://our.umbraco.com/";
            var ourBadgeUrl = $"{ourUrl}community/badges/#c-trib";
            var ourEditProfileUrl = $"{ourUrl}member/profile";

            var sb = new StringBuilder();

            sb.AppendLine("Hi @" + issue.User.Login + ",");
            sb.AppendLine();

            sb.Append($"We really wish to give you the [**Contributors badge**]({ourBadgeUrl}) badge on **Our Umbraco**, ");
            sb.Append("but we haven't been able to find an account on Our Umbraco matching your GitHub ID.");
            sb.AppendLine();
            sb.AppendLine();

            sb.Append("If you haven't already linked your Our Umbraco account with your GitHub account, you ");
            sb.Append($"can do so by going to the [**Edit profile**]({ourEditProfileUrl}) and click the **Link with GitHub** button.");
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine("Best regards,");
            sb.AppendLine("The Friendly Umbraco Robot");

            return sb.ToString().Trim();

        }

        private string GetContributorBadgeAddedMessage(Issue issue, IMember member)
        {

            string ourUrl = "https://our.umbraco.com/";
            string ourMemberUrl = $"{ourUrl}members/id:{member.Id}/";
            string ourBadgeUrl = $"{ourUrl}community/badges/#c-trib";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Hi @" + issue.User.Login + ",");
            sb.AppendLine();

            sb.Append("As a appreciation for your merged PR, you have now been given the");
            sb.Append($"[**Contributors badge**]({ourBadgeUrl}) badge on **Our Umbraco** - the official Umbraco community forum.");
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine("Best regards,");
            sb.AppendLine("The friendly Our bot");

            return sb.ToString().Trim();

        }

    }

}