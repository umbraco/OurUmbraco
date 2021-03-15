namespace OurUmbraco.Community.Nuget
{
    using System;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class NugetRegistrationItemEntry
    {
        [JsonProperty("catalogEntry")]
        public NugetRegistrationCatalogEntry CatalogEntry { get; set; }

        [JsonProperty("commitTimeStamp")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime CommitTimeStamp { get; set; }

    }
}
