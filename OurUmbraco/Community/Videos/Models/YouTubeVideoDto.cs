using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OurUmbraco.Community.Videos.Models
{
    [DataContract(Name = "youTubeVideo")]
    public class YouTubeVideoDto
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "length")]
        public Double Length { get; set; }

        [DataMember(Name = "publishedAt")]
        public DateTime PublishedAt { get; set; }

        [DataMember(Name = "likes")]
        public long Likes { get; set; }

        [DataMember(Name = "plays")]
        public long Plays { get; set; }

        [DataMember(Name = "tags")]
        public IEnumerable<string> Tags { get; set; }

        [DataMember(Name = "thumbnailUrl")]
        public string ThumbnailUrl { get; set; }
    }
}
