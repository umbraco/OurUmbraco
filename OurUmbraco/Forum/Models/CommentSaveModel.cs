using System.Runtime.Serialization;

namespace OurUmbraco.Forum.Models
{
    [DataContract( Name="comment")]
    public class CommentSaveModel
    {
        [DataMember( Name = "body")]
        public string Body { get; set; }

        [DataMember(Name = "topic")]
        public int Topic { get; set; }

        [DataMember(Name = "parent")]
        public int Parent { get; set; }

    }
}
