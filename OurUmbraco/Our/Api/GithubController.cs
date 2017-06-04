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
        /// <summary>
        /// Get all contributors from GitHub Umbraco repositories
        /// </summary>
        /// <returns></returns>
        public IRestResponse<List<GitHubContributorModel>> GetAllContributors()
        {
            //https://api.github.com/repos/umbraco/Umbraco-CMS/contributors
            var client = new RestClient("https://api.github.com");
            var request = new RestRequest("/repos/umbraco/Umbraco-CMS/contributors", Method.GET);
            client.UserAgent = "OurUmbraco";

            var response = client.Execute<List<GitHubContributorModel>>(request);
            return response;
        }
    }
}
