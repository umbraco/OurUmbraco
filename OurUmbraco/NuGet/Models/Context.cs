using Newtonsoft.Json;

namespace OurUmbraco.NuGet.Models
{
    public class Context
    {
        [JsonProperty("@vocab")]
        public string Vocab { get; set; }

        [JsonProperty("@base")]
        public string Base { get; set; }
    }
}