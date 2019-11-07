namespace OurUmbraco.Community.Nuget
{
    using Newtonsoft.Json;

    public class NugetSearchResult
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("totalDownloads", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalDownloads { get; set; }
    }
}
