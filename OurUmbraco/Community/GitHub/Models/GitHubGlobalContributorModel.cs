using System.Collections.Generic;
using System.Linq;

namespace OurUmbraco.Community.GitHub.Models
{

    public class GitHubCachedGlobalContributorModel
    {

        public int AuthorId { get; set; }

        public string AuthorLogin { get; set; }

        public string AuthorUrl { get; set; }

        public string AuthorAvatarUrl { get; set; }

        public int TotalCommits { get; set; }

        public int TotalAdditions { get; set; }

        public int TotalDeletions { get; set; }

        public GitHubCachedGlobalContributorModel() { }

        public GitHubCachedGlobalContributorModel(GitHubGlobalContributorModel contributor)
        {
            AuthorId = contributor.Author.Id;
            AuthorLogin = contributor.Author.Login;
            AuthorUrl = contributor.Author.HtmlUrl;
            AuthorAvatarUrl = contributor.Author.AvatarUrl;
            TotalCommits = contributor.TotalCommits;
            TotalAdditions = contributor.TotalAdditions;
            TotalDeletions = contributor.TotalDeletions;
        }

    }

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