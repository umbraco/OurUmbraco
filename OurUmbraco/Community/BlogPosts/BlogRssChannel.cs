using Newtonsoft.Json;

namespace OurUmbraco.Community.BlogPosts {

    public class BlogRssChannel {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

    }

}