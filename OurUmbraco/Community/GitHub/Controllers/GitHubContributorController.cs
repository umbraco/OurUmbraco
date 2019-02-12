using OurUmbraco.Community.GitHub.Models;
using OurUmbraco.Community.GitHub.Models.Cached;
using System;
using System.Linq;
using System.Web.Mvc;
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
            var model = new GitHubContributorsModel { Contributors = Enumerable.Empty<GitHubCachedGlobalContributorModel>() };

            try
            {
                model.Contributors = new GitHubPullRequestContributorsService()
                    .Contributors()
                    .Select(c => new GitHubCachedGlobalContributorModel(c));
            }
            catch (Exception ex)
            {
                LogHelper.Error<GitHubContributorController>("Unable to load GitHub contributors", ex);
            }
            
            return PartialView("~/Views/Partials/Home/GitHubContributors.cshtml", model);
        }
    }
}