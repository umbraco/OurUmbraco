using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OurUmbraco.NuGet.Models
{
    public class VersionDetails
    {
        [JsonProperty("@id")]
        public string Id;

        [JsonProperty("@type")]
        public List<string> Type;

        [JsonProperty("catalogEntry")]
        public string CatalogEntry;

        [JsonProperty("listed")]
        public bool Listed;

        [JsonProperty("packageContent")]
        public string PackageContent;

        [JsonProperty("published")]
        public DateTime Published;

        [JsonProperty("registration")]
        public string Registration;
    }
}