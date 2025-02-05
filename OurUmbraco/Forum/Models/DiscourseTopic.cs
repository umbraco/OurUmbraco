using Newtonsoft.Json;

namespace OurUmbraco.Forum.Models
{
    public class DiscourseTopic
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("reply_count")]
        public int ReplyCount { get; set; }

        [JsonProperty("visible")]
        public bool Visible { get; set; }

        [JsonProperty("closed")]
        public bool Closed { get; set; }

        [JsonProperty("archived")]
        public bool Archived { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("category_id")]
        public int CategoryId { get; set; }

        public string RedirectUrl => System.Configuration.ConfigurationManager.AppSettings["DiscourseApiBaseUrl"] + $"t/{Slug}/{Id}";
    }
}