using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using OurUmbraco.Community.GitHub.Models;
using OurUmbraco.Community.GitHub.Models.Cached;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Community.GitHub.Controllers
{
    public class GitHubContributorController : SurfaceController
    {
        /// <summary>
        /// Gets data for all GitHub contributors for all listed Umbraco repositories,
        /// excluding the GitHub IDs of the HQ contributors from the text file list
        /// </summary>
        /// <returns></returns>
        public ActionResult GitHubGetContributorsResult(bool force = false)
        {
            var model = new GitHubContributorsModel { Contributors = new List<GitHubCachedGlobalContributorModel>() };
            var service = new GitHubService();

            if (force)
            {
                try
                {
                    // Update the cached contributors
                    var result = service.UpdateOverallContributors();

                    // Update the model with the contributors
                    model.Contributors = result.Contributors.Select(x => new GitHubCachedGlobalContributorModel(x)).ToList();
                }
                catch (Exception ex)
                {
                    // Log the error so we can debug it later
                    LogHelper.Error<GitHubContributorController>("Unable to load GitHub contributors from the GitHub", ex);
                }
            }
            else if (System.IO.File.Exists(service.JsonPath))
            {
                try
                {
                    // Update the model with the contributors
                    model.Contributors = service.GetOverallContributorsFromDisk();
                }
                catch (Exception ex)
                {
                    // Log the error so we can debug it later
                    LogHelper.Error<GitHubContributorController>("Unable to load GitHub contributors from the GitHub API or cache", ex);
                }
            }

            return PartialView("~/Views/Partials/Home/GitHubContributors.cshtml", model);
        }
    }
}