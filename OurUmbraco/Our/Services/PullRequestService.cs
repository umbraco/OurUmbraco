using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Newtonsoft.Json;
using OurUmbraco.Community.GitHub.Models;
using OurUmbraco.Our.Models;

namespace OurUmbraco.Our.Services
{
    public class PullRequestService
    {
        private readonly string _jsonPath = HostingEnvironment.MapPath("~/App_Data/TEMP/GithubPullRequests.json");
        private readonly string _hqUsersFile = HostingEnvironment.MapPath("~/Config/githubhq.txt");
        private readonly string _teamUmbracoUsersFile = HostingEnvironment.MapPath("~/Config/TeamUmbraco.json");

        private TeamUmbraco GetTeam(string repository)
        {
            var teamUmbraco = new TeamUmbraco();
            var usernames = File.ReadAllLines(_hqUsersFile).Where(x => x.Trim() != "").Distinct().ToArray();

            var content = File.ReadAllText(_teamUmbracoUsersFile);
            var teamUmbracoUsers = JsonConvert.DeserializeObject<List<TeamUmbraco>>(content);
            var team = teamUmbracoUsers.FirstOrDefault(x => x.TeamName == repository);
            if (team != null)
            {
                team.Members.AddRange(usernames);
                teamUmbraco = team;
            }

            return teamUmbraco;
        }

        public List<string> GetHqMembers()
        {
            if (!File.Exists(_hqUsersFile))
            {
                var message = $"Config file was not found: {_hqUsersFile}";
                LogHelper.Debug<GitHubService>(message);
                throw new Exception(message);
            }

            var hqUsernames = File.ReadAllLines(_hqUsersFile).Where(x => x.Trim() != "").Distinct().ToArray();
            var hqMembers = hqUsernames.Select(hqUsername => hqUsername.ToLowerInvariant()).ToList();
            return hqMembers;
        }

        internal List<string> GetTeamMembers()
        {
            var allTeamMembers = new List<string>();
            var content = File.ReadAllText(_teamUmbracoUsersFile);
            var teamUmbracoUsers = JsonConvert.DeserializeObject<List<TeamUmbraco>>(content);
            foreach (var team in teamUmbracoUsers)
            {
                foreach (var member in team.Members)
                {
                    if (allTeamMembers.Contains(member) == false)
                        allTeamMembers.Add(member.ToLowerInvariant());
                }
            }

            return allTeamMembers;
        }

        private GithubPullRequestModel GetPullRequest(OurUmbraco.Community.Models.Repository repository, string pullRequestNumber)
        {
            GithubPullRequestModel prModel = null;

            var store = repository.IssuesStorageDirectory() + "/pulls/";
            var issueFile = store + pullRequestNumber + ".pull.combined.json";
            var issueFilePath = HostingEnvironment.MapPath(issueFile);
            if (File.Exists(issueFilePath))
            {
                var content = File.ReadAllText(issueFilePath);
                prModel = JsonConvert.DeserializeObject<GithubPullRequestModel>(content);
            }

            return prModel;
        }

        public GithubPullRequestComment GetFirstTeamComment(OurUmbraco.Community.Models.Repository repository, string pullRequestNumber)
        {
            GithubPullRequestComment firstTeamCommment = null;
            var pr = GetPullRequest(repository, pullRequestNumber);

            if (pr != null)
            {
                var users = GetTeam(repository.Slug).Members.Select(x => x.ToLower());
                var foundComment = pr.Comments.OrderBy(x => x.created_at).FirstOrDefault(x => users.Contains(x.user.Login.ToLower()));
                if (foundComment != null)
                    firstTeamCommment = foundComment;
            }

            return firstTeamCommment;
        }

        public List<GithubPullRequestModel> GetPullsNonHq(string repository)
        {
            var content = File.ReadAllText(_jsonPath);
            var pulls = JsonConvert.DeserializeObject<List<GithubPullRequestModel>>(content).Where(x => x.Repository == repository);
            var hqList = File.ReadAllText(_hqUsersFile).Split('\n');
            var pullsNonHq = new List<GithubPullRequestModel>();
            foreach (var pull in pulls)
            {
                if (pull?.User?.Login == null)
                    continue;

                var isHq = hqList.Any(
                    y => string.Equals(y.Trim(), pull.User.Login, StringComparison.InvariantCultureIgnoreCase));
                if (isHq == false)
                    pullsNonHq.Add(pull);
            }

            pullsNonHq = pullsNonHq.ToList();
            return pullsNonHq;
        }

        public List<PullRequestContributor> GetLeaderBoard(DateTime fromDate, DateTime toDate, string repository = "Umbraco-CMS")
        {
            var pullsNonHq = GetPullsNonHq(repository);
            var contributors = new List<PullRequestContributor>();

            foreach (var pr in pullsNonHq.Where(x => x.CreatedAt >= fromDate && x.CreatedAt <= toDate))
            {
                var isOpen = pr.ClosedAt == null && pr.MergedAt == null;
                var isClosed = pr.ClosedAt != null && pr.MergedAt == null;
                var isMerged = pr.ClosedAt != null && pr.MergedAt != null;

                var contributor = contributors.FirstOrDefault(x => string.Equals(x.Username, pr.User.Login, StringComparison.InvariantCultureIgnoreCase));
                if (contributor == null)
                {
                    contributor = new PullRequestContributor
                    {
                        Username = pr.User.Login,
                        Contributions = 1,
                        OpenContributions = isOpen ? 1 : 0,
                        ClosedContributions = isClosed ? 1 : 0,
                        MergedContributions = isMerged ? 1 : 0
                    };
                    contributors.Add(contributor);
                }
                else
                {
                    contributor.Contributions = contributor.Contributions + 1;
                    contributor.OpenContributions = isOpen ? contributor.OpenContributions + 1 : contributor.OpenContributions;
                    contributor.ClosedContributions = isClosed ? contributor.ClosedContributions + 1 : contributor.ClosedContributions;
                    contributor.MergedContributions = isMerged ? contributor.MergedContributions + 1 : contributor.MergedContributions;
                }
            }

            return contributors;
        }
    }
}
