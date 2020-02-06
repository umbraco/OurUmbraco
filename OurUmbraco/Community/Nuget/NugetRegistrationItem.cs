namespace OurUmbraco.Community.Nuget
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class NugetRegistrationItem
    {
        [JsonProperty("items")]
        public IEnumerable<NugetRegistrationItemEntry> Items { get; set; }
    }
}
