using System.Collections.Generic;
using OurUmbraco.Community.GitHub.Models.Cached;

namespace OurUmbraco.Community.GitHub.Models
{
    public class GitHubContributorsModel : IGitHubContributorsModel
    {
        public IEnumerable<GitHubCachedGlobalContributorModel> Contributors { get; set; }
    }
}