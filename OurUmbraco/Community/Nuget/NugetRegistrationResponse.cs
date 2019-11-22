namespace OurUmbraco.Community.Nuget
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class NugetRegistrationResponse
    {
        [JsonProperty("items")]
        public IEnumerable<NugetRegistrationItem> Items { get; set; }
    }
}
