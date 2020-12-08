namespace OurUmbraco.Community.Nuget
{
    using System;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class NugetRegistrationCatalogEntry
    {
        [JsonProperty("published")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime PublishedDate { get; set; }
    }
}
