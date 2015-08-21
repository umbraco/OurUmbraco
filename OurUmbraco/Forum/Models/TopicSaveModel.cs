using System.Runtime.Serialization;

namespace OurUmbraco.Forum.Models
{
    [DataContract(Name = "topic")]
    public class TopicSaveModel
    {
        [DataMember(Name = "body")]
        public string Body { get; set; }

        [DataMember(Name = "forum")]
        public int Forum { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "version")]
        public int Version { get; set; }

    }
}
