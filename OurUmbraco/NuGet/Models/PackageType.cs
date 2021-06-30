using Newtonsoft.Json;

namespace OurUmbraco.NuGet.Models
{
    public class PackageType
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}