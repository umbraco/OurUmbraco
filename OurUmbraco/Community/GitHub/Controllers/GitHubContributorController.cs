using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<ActionResult> GitHubGetContributorsResult(bool force = false)
        {
            var model = new GitHubContributorsModel { Contributors = new List<GitHubCachedGlobalContributorModel>() };
            
            try
            {
                // Update the cached contributors
                var r = new GitHubPullRequestContributorsService().Contributors();
                //var result = await service.UpdateOverallContributors();

                // Update the model with the contributors
                model.Contributors = r.OrderByDescending(res => res.PullRequestCount).ThenByDescending(res => res.Additions).ThenByDescending(res => res.Deletions).Select(res => new GitHubCachedGlobalContributorModel { AuthorLogin = res.Username, TotalCommits = res.PullRequestCount, AuthorUrl = res.Url, AuthorAvatarUrl = res.AvatarUrl, TotalAdditions = res.Additions, TotalDeletions = res.Deletions}).ToList();
            }
            catch (Exception ex)
            {
                // Log the error so we can debug it later
                LogHelper.Error<GitHubContributorController>("Unable to load GitHub contributors from the GitHub", ex);
            }
           

            return PartialView("~/Views/Partials/Home/GitHubContributors.cshtml", model);
        }
    }
}