using Newtonsoft.Json;
using System;

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

        [JsonProperty("post_url")]
        public string PostUrl { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("last_poster_username")]
        public string LastPosterUsername { get; set; }

        [JsonProperty("last_posted_at")]
        public DateTime LastPostedAt { get; set; }

        public string AuthorAvatar { get; set; }
        public string LastUpdatedFriendly { get; set; }
        public string ForumCategory { get; set; }
        
        public string AuthorName;
        public string RedirectUrl => System.Configuration.ConfigurationManager.AppSettings["DiscourseApiBaseUrl"] + $"t/{Slug}/{Id}";
    }
}