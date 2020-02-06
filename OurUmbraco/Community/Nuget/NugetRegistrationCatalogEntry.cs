namespace OurUmbraco.Community.Nuget
{
    using System;

    using Newtonsoft.Json;

    public class NugetRegistrationCatalogEntry
    {
        [JsonProperty("published")]
        public DateTime PublishedDate { get; set; }
    }
}
