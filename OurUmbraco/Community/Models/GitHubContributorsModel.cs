using System.Collections.Generic;
using OurUmbraco.Community.Controllers;

namespace OurUmbraco.Community.Models
{
    public class GitHubContributorsModel : IGitHubContributorsModel
    {
        public List<GitHubCachedGlobalContributorModel> Contributors { get; set; }
    }
}