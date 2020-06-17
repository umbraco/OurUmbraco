using Newtonsoft.Json;
using System;

namespace OurUmbraco.Documentation.Models
{

    public class AzureDevopsArtifacts
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("value")]
        public Artifact[] Artifacts { get; set; }
    }

    public class Artifact
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("source")]
        public Guid Source { get; set; }

        [JsonProperty("resource")]
        public Resource Resource { get; set; }
    }

    public partial class Resource
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("downloadUrl")]
        public Uri DownloadUrl { get; set; }
    }
}
