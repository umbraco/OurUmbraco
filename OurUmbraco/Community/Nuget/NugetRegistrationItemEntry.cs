namespace OurUmbraco.Community.Nuget
{
    using System;

    using Newtonsoft.Json;

    public class NugetRegistrationItemEntry
    {
        [JsonProperty("catalogEntry")]
        public NugetRegistrationCatalogEntry CatalogEntry { get; set; }

        [JsonProperty("commitTimeStamp")]
        public DateTime CommitTimeStamp { get; set; }

    }
}
