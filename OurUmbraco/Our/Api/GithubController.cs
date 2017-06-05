using System.Collections.Generic;
using OurUmbraco.Community.Models;
using RestSharp;

namespace OurUmbraco.Our.Api
{
    public class GitHubController
    {
        private const string RepositoryOwner = "Umbraco";
        private const string GitHubApiClient = "https://api.github.com";
        private const string UserAgent = "OurUmbraco";

        /// <summary>
        /// Get all contributors from GitHub Umbraco repositories
        /// </summary>
        /// <param name="repo"></param>
        /// <returns></returns>
        public IRestResponse<List<GitHubContributorModel>> GetAllRepoContributors(string repo)
        {
            var client = new RestClient(GitHubApiClient);
            var request = new RestRequest(string.Format("/repos/{0}/{1}/stats/contributors", RepositoryOwner, repo), Method.GET);
            client.UserAgent = UserAgent;
            var response = client.Execute<List<GitHubContributorModel>>(request);
            return response;
        }
    }
}
