using System.Collections.Generic;

namespace OurUmbraco.Community.Models
{
    public class GitHubContributorsModel : IGitHubContributorsModel
    {
        public IEnumerable<GitHubContributorModel> Contributors { get; set; }
    }
}