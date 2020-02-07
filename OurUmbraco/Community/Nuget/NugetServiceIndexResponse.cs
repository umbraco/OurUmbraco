namespace OurUmbraco.Community.Nuget
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class NugetServiceIndexResponse
    {
        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty("resources", NullValueHandling = NullValueHandling.Ignore)]
        public List<NugetServiceIndexResource> Resources { get; set; }
    }
}
