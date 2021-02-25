using System;
using System.Runtime.Serialization;

namespace OurUmbraco.Community.Twitter.Models
{
    [DataContract(Name = "tweet")]
    public class TweetDto
    {
        [DataMember(Name = "id")]
        public long Id { get; set; }

        [DataMember(Name = "screenName")]
        public string ScreenName { get; set; }

        [DataMember(Name = "profileImageUrl")]
        public string ProfileImageUrl { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "createdDate")]
        public DateTime CreatedDate { get; set; }
    }
}
