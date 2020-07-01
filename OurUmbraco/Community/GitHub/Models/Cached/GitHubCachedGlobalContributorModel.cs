namespace OurUmbraco.Community.GitHub.Models.Cached
{
    public class GitHubCachedGlobalContributorModel
    {
        public long AuthorId { get; set; }

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
}