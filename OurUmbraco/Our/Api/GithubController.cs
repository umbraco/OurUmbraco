using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OurUmbraco.Community.Models;
using RestSharp;

namespace OurUmbraco.Our.Api
{
    public class GitHubController
    {    
        private const string RepositoryOwner = "Umbraco";
        /// <summary>
        /// Get all contributors from GitHub Umbraco repositories
        /// </summary>
        /// <returns></returns>
        public IRestResponse<List<GitHubContributorModel>> GetAllRepoContributors(string repo)
        {
            var client = new RestClient("https://api.github.com");
            var request = new RestRequest(string.Format("/repos/{0}/{1}/stats/contributors", RepositoryOwner, repo), Method.GET);
            client.UserAgent = "OurUmbraco";
            var response = client.Execute<List<GitHubContributorModel>>(request);
            return response;
        }
    }
}
