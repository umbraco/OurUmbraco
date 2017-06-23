using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using OurUmbraco.Community.Models;
using OurUmbraco.Our.Api;
using RestSharp;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;

namespace OurUmbraco.Community.Controllers
{
    public class GitHubContributorController : SurfaceController
    {
        /// <summary>
        /// Repositories to include in the combination of contributions
        /// </summary>
        private readonly string[] Repositories =
        {
            "Umbraco-CMS",
            "UmbracoDocs",
            "OurUmbraco",
            "Umbraco.Deploy.Contrib",
            "Umbraco.Courier.Contrib",
            "Umbraco.Deploy.ValueConnectors"
        };

        protected string JsonPath
        {
            get { return Server.MapPath("~/App_Data/TEMP/GithubContributors.json"); }
        }

        public List<GitHubCachedGlobalContributorModel> GetCachedContributors()
        {

            if (!System.IO.File.Exists(JsonPath)) throw new Exception("The JSON file doesn't exist on disk");


            List<GitHubCachedGlobalContributorModel> temp = new List<GitHubCachedGlobalContributorModel>();

            // Parse the JSON file from disk
            foreach (JToken token in Skybrud.Essentials.Json.JsonUtils.LoadJsonArray(JsonPath))
            {

                temp.Add(token.ToObject<GitHubCachedGlobalContributorModel>());

            }

            return temp;

        }

        /// <summary>
        /// Gets data for all GitHub contributors for all listed Umbraco repositories, 
        /// excluding the GitHub IDs of the HQ contributors from the text file list
        /// </summary>
        /// <returns></returns>
        public ActionResult GitHubGetContributorsResult(bool force = false)
        {
            var model = new GitHubContributorsModel();
            model.Contributors = new List<GitHubCachedGlobalContributorModel>();

            try
            {

                // If the file exists on disk and hasn't expired (AKA not older than one day), we should just load that
                if (force == false && System.IO.File.Exists(JsonPath) && System.IO.File.GetLastWriteTimeUtc(JsonPath) >= DateTime.UtcNow.AddDays(-1))
                {
                    model.Contributors = GetCachedContributors();
                    return PartialView("~/Views/Partials/Home/GitHubContributors.cshtml", model);
                }

                try
                {
                    // Load the contributors via the GitHub API and map to the cached model
                    var contributors = GetContributors().Select(x => new GitHubCachedGlobalContributorModel(x));

                    // Serialize the contributors to raw JSON
                    string rawJson = JsonConvert.SerializeObject(contributors, Formatting.Indented);

                    // Save the JSON to disk
                    System.IO.File.WriteAllText(JsonPath, rawJson, Encoding.UTF8);

                    model.Contributors = contributors.ToList();
                }
                catch (Exception ex)
                {
                    // Log the error so we can debug it later
                    LogHelper.Error<GitHubContributorController>("Unable to load GitHub contributors from the GitHub API", ex);

                    // Load the contributors from the cache (if we have any)
                    model.Contributors = GetCachedContributors();
                }

            }
            catch (Exception ex)
            {
                // Log the error so we can debug it later
                LogHelper.Error<GitHubContributorController>("Unable to load GitHub contributors from the GitHub API", ex);

            }

            return PartialView("~/Views/Partials/Home/GitHubContributors.cshtml", model);
        }

        public List<GitHubGlobalContributorModel> GetContributors()
        {


            string configPath = Server.MapPath("~/config/githubhq.txt");
            if (!System.IO.File.Exists(configPath))
            {
                LogHelper.Debug<GitHubContributorController>("Config file was not found: " + configPath);
                throw new Exception("Config file was not found: " + configPath);
            }

            string[] login = System.IO.File.ReadAllLines(configPath).Where(x => x.Trim() != "").Distinct().ToArray();

            var githubController = new GitHubController();
            var gitHubContributors = new List<GitHubContributorModel>();
            foreach (var repo in Repositories)
            {
                var response = githubController.GetAllRepoContributors(repo);
                if (response.StatusCode == HttpStatusCode.OK &&
                    response.ResponseStatus == ResponseStatus.Completed)
                {
                    gitHubContributors.AddRange(response.Data);
                }
                else
                {
                    LogHelper.Warn<IGitHubContributorsModel>(string.Format("Invalid HTTP response for repository {0}", repo));
                }
            }

            // filter to only include items from the last year (if we ran 4.6.1 we could have used ToUnixTimeSeconds())
            var filteredRange = DateTime.UtcNow.AddYears(-1).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            foreach (var contrib in gitHubContributors)
            {
                var contribWeeks = contrib.Weeks.Where(x => x.W >= filteredRange).ToList();
                contrib.TotalAdditions = contribWeeks.Sum(x => x.A);
                contrib.TotalDeletions = contribWeeks.Sum(x => x.D);
                contrib.Total = contribWeeks.Sum(x => x.C);
            }

            var filteredContributors = gitHubContributors
                .Where(g => !login.Contains(g.Author.Login))
                .GroupBy(g => g.Author.Id)
                .OrderByDescending(c => c.Sum(g => g.Total));

            List<GitHubGlobalContributorModel> temp = new List<GitHubGlobalContributorModel>();

            foreach (var group in filteredContributors)
            {
                var contributor = new GitHubGlobalContributorModel(group);
                if (contributor.TotalCommits > 0)
                {
                    temp.Add(contributor);
                }
            }

            return temp;


        }

    }
}
