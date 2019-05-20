using System;
using Newtonsoft.Json;

namespace OurUmbraco.Our.Models.GitHub
{
    public class Comments
    {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreateDateTime { get; set; }

        // Note: leaving the other properties commented out in case we need them later

        //public string url { get; set; }
        //public string html_url { get; set; }
        //public string issue_url { get; set; }
        //public int id { get; set; }
        //public string node_id { get; set; }

        //public DateTime updated_at { get; set; }
        //public string author_association { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

    }
}
