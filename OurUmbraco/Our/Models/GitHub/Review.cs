using System;
using Newtonsoft.Json;

namespace OurUmbraco.Our.Models.GitHub
{
    public class Review
    {
        public int id { get; set; }
        public string node_id { get; set; }

        [JsonProperty("user")]
        public User Actor { get; set; }
        
        public string body { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        public string html_url { get; set; }
        public string pull_request_url { get; set; }
        public string author_association { get; set; }
        public _Links _links { get; set; }

        [JsonProperty("submitted_at")]
        public DateTime CreateDateTime { get; set; }
        
        public string commit_id { get; set; }
    }

    public class _Links
    {
        public Html html { get; set; }
        public Pull_Request pull_request { get; set; }
    }

    public class Html
    {
        public string href { get; set; }
    }

    public class Pull_Request
    {
        public string href { get; set; }
    }
}
