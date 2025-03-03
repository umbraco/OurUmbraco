using Newtonsoft.Json;
using System.Collections.Generic;

namespace OurUmbraco.Forum.Models
{
    public class TopicListModel
    {
        [JsonProperty("users")]
        public List<DiscourseUser> Users { get; set; }

        [JsonProperty("topic_list")]
        public DiscourseTopicList TopicList { get; set; }
    }
}