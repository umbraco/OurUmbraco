using System.Collections.Generic;
using System.Linq;

namespace OurUmbraco.Community.GitHub.Models
{
    public class GitHubGlobalContributorModel
    {
        public List<GitHubContributorModel> Items { get; set; }

        public int Id
        {
            get { return Author.Id; }
        }

        public int TotalCommits
        {
            get { return Items.Sum(x => x.Total); }
        }

        public int TotalAdditions
        {
            get { return Items.Sum(x => x.TotalAdditions); }
        }

        public int TotalDeletions
        {
            get { return Items.Sum(x => x.TotalDeletions); }
        }

        public Author Author
        {
            get { return Items.First().Author; }
        }

        public GitHubGlobalContributorModel(IEnumerable<GitHubContributorModel> items)
        {
            Items = items.ToList();
        }

    }
}