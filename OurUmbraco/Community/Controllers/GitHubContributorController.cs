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

        /// <summary>
        /// Gets data for all GitHub contributors for all listed Umbraco repositories, 
        /// excluding the GitHub IDs of the HQ contributors from the text file list
        /// </summary>
        /// <returns></returns>
        public ActionResult GitHubGetContributorsResult()
        {
            var model = new GitHubContributorsModel();
            try
            {
                string configPath = Server.MapPath("~/config/githubhq.txt");
                if (!System.IO.File.Exists(configPath))
                {
                    LogHelper.Debug<GitHubContributorController>("Config file was not found: " + configPath);
                    return PartialView("~/Views/Partials/Home/GitHubContributors.cshtml", model);
                }

                string[] login = System.IO.File.ReadAllLines(configPath).Where(x => x.Trim() != "").Distinct().ToArray();
                var contributors = ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<List<GitHubGlobalContributorModel>>("UmbracoGitHubContributors",
                    () =>
                    {
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

                        var filteredContributors = gitHubContributors
                            .Where(g => !login.Contains(g.Author.Login))
                            .GroupBy(g => g.Author.Id)
                            .OrderByDescending(c => c.Sum(g => g.Total));

                        List<GitHubGlobalContributorModel> temp = new List<GitHubGlobalContributorModel>();

                        foreach (var group in filteredContributors)
                        {
                            temp.Add(new GitHubGlobalContributorModel(group));
                        }

                        return temp;

                    }, TimeSpan.FromDays(1));


                model.Contributors = contributors;
            }
            catch (Exception ex)
            {
                LogHelper.Error<IGitHubContributorsModel>("Could not get GitHub Contributors", ex);
            }

            return PartialView("~/Views/Partials/Home/GitHubContributors.cshtml", model);
        }
    }
}
