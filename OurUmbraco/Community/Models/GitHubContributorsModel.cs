using System.Collections.Generic;
using System.Linq;

namespace OurUmbraco.Community.Models
{
    public class GitHubContributorsModel : IGitHubContributorsModel
    {
        public IEnumerable<IGrouping<int, GitHubContributorModel>> Contributors { get; set; }
    }
}