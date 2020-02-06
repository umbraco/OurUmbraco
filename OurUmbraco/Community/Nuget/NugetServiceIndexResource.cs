namespace OurUmbraco.Community.Nuget
{
    using Newtonsoft.Json;

    public class NugetServiceIndexResource
    {
        [JsonProperty("@id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("@type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("comment", NullValueHandling = NullValueHandling.Ignore)]
        public string Comment { get; set; }
    }
}
