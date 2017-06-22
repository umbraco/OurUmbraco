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
using Umbraco.Web.PropertyEditors;

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

                        // filter to only include items from the last year
                        var filteredRange = DateTime.UtcNow.AddYears(-1).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                        foreach (var contrib in gitHubContributors)
                        {
                            int add = 0, del = 0, total = 0;
                            foreach (var wk in contrib.Weeks.Where(x => x.W >= filteredRange))
                            {
                                add += wk.A;
                                del += wk.D;
                                total += wk.C;
                            }
                            contrib.Total = total;
                            contrib.TotalAdditions = add;
                            contrib.TotalDeletions = del;
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
                                temp.Add(contributor);
                        }

                        return temp;

                    }, TimeSpan.FromSeconds(1));


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
