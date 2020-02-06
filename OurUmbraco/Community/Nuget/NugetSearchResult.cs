namespace OurUmbraco.Community.Nuget
{
    using Newtonsoft.Json;

    public class NugetSearchResult
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("totalDownloads", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalDownloads { get; set; }

        [JsonProperty("registration", NullValueHandling = NullValueHandling.Ignore)]
        public string PackageRegistrationUrl { get; set; }
    }
}
