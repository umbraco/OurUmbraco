using Newtonsoft.Json;

namespace OurUmbraco.Forum.Models
{
    public class DiscourseUser
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("avatar_template")]
        public string AvatarTemplate { get; set; }

        [JsonProperty("trust_level")]
        public int TrustLevel { get; set; }

        [JsonProperty("flair_name")]
        public string FlairName { get; set; }

        [JsonProperty("flair_url")]
        public string FlairUrl { get; set; }

        [JsonProperty("flair_group_id")]
        public int? FlairGroupId { get; set; }

        [JsonProperty("admin")]
        public bool? Admin { get; set; }

        [JsonProperty("moderator")]
        public bool? Moderator { get; set; }
    }
}
