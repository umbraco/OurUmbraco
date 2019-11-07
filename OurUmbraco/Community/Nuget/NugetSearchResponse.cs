namespace OurUmbraco.Community.Nuget
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class NugetSearchResponse
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public List<NugetSearchResult> Results { get; set; }
    }
}
