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
    }
}
