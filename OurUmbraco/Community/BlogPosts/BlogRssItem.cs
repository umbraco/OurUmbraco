using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OurUmbraco.Community.BlogPosts {
    
    public class BlogRssItem {

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("guid")]
        public string Guid { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("pubDate")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTimeOffset PublishedDate { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        public bool HasDescription
        {
            get { return !String.IsNullOrWhiteSpace(Description); }
        }

        [JsonProperty("channel")]
        public BlogRssChannel Channel { get; set; }

    }

}