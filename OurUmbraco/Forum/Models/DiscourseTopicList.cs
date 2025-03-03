using Newtonsoft.Json;
using System.Collections.Generic;

namespace OurUmbraco.Forum.Models
{
    public class DiscourseTopicList
    {
        [JsonProperty("topics")]
        public List<DiscourseTopic> Topics { get; set; }
    }
}
