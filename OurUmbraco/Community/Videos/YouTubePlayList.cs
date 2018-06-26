using System.Collections.Generic;
using Newtonsoft.Json;

namespace OurUmbraco.Community.Videos
{
    public class YouTubeInfo
    {
        public Playlist[] PlayLists { get; set; }
    }

    public class Playlist
    {
        public string Id { get; set; }
        public string Title { get; set; }

        [JsonProperty("members")]
        public List<int> MemberIds { get; set; }
    }
}
