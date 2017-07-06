using System.Collections.Generic;
using OurUmbraco.Community.GitHub.Models.Cached;

namespace OurUmbraco.Community.GitHub.Models
{
    public class GitHubContributorsModel : IGitHubContributorsModel
    {
        public List<GitHubCachedGlobalContributorModel> Contributors { get; set; }
    }
}