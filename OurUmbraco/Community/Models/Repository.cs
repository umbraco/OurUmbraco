namespace OurUmbraco.Community.Models
{
    public class Repository
    {
        public string Alias { get; }

        public string Owner { get; }

        public string Slug { get; }

        public string Name { get; }

        public Repository(string alias, string owner, string slug, string name)
        {
            Alias = alias;
            Owner = owner;
            Slug = slug;
            Name = name;
        }

        public string IssuesStorageDirectory()
        {
            return $"~/App_Data/TEMP/GitHub/{Owner}__{Alias}/issues";
        }

        public string PullsStorageDirectory()
        {
            return $"{IssuesStorageDirectory()}/pulls";
        }
    }
}