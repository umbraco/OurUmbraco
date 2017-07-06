using System.Collections.Generic;

namespace OurUmbraco.Community.GitHub.Models
{
    public class GitHubContributorsModel : IGitHubContributorsModel
    {
        public List<GitHubCachedGlobalContributorModel> Contributors { get; set; }
    }
}