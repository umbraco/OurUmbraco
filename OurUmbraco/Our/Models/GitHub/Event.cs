using System;
using Newtonsoft.Json;

namespace OurUmbraco.Our.Models.GitHub
{
    public class Event
    {
        [JsonProperty("event")]
        public string Name { get; set; }

        [JsonProperty("label")]
        public Label Label { get; set; }

        [JsonProperty("actor")]
        public User Actor { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreateDateTime { get; set; }

        [JsonProperty("assigner")]
        public User Assigner { get; set; }

        // Note: leaving the other properties commented out in case we need them later

        //public int id { get; set; }
        //public string node_id { get; set; }
        //public string url { get; set; }
        //public object commit_id { get; set; }
        //public object commit_url { get; set; }
        //public User assignee { get; set; }

        //public Milestone milestone { get; set; }
        //public class Milestone
        //{
        //    public string title { get; set; }
        //}
    }
}
