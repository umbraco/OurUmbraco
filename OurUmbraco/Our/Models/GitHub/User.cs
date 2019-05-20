using Newtonsoft.Json;

namespace OurUmbraco.Our.Models.GitHub
{
    public class User
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        // Note: leaving the other properties commented out in case we need them later

        //public int id { get; set; }
        //public string node_id { get; set; }
        //public string gravatar_id { get; set; }
        //public string url { get; set; }
        //public string html_url { get; set; }
        //public string followers_url { get; set; }
        //public string following_url { get; set; }
        //public string gists_url { get; set; }
        //public string starred_url { get; set; }
        //public string subscriptions_url { get; set; }
        //public string organizations_url { get; set; }
        //public string repos_url { get; set; }
        //public string events_url { get; set; }
        //public string received_events_url { get; set; }
        //public string type { get; set; }
        //public bool site_admin { get; set; }
    }
}
