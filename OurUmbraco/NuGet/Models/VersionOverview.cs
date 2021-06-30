using Newtonsoft.Json;

namespace OurUmbraco.NuGet.Models
{
    public class VersionOverview
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("downloads")]
        public int Downloads { get; set; }

        [JsonProperty("@id")]
        public string Id { get; set; }
    }
}