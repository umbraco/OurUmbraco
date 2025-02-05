using Newtonsoft.Json;
using System;

namespace OurUmbraco.Forum.Models
{
    public class DiscourseCreateTopic
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("raw")]
        public string Raw { get; set; }

        [JsonProperty("category")]
        public int Category { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("embed_url")]
        public string EmbedUrl { get; set; }

        [JsonProperty("external_id")]
        public int ExternalId { get; set; }
    }
}
