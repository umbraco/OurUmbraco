namespace OurUmbraco.Community.Nuget
{
    using Newtonsoft.Json;

    public class NugetRegistrationItemEntry
    {
        [JsonProperty("catalogEntry")]
        public NugetRegistrationCatalogEntry CatalogEntry { get; set; }
    }
}
