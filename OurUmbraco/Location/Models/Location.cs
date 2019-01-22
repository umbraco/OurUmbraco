using Newtonsoft.Json;

namespace OurUmbraco.Location.Models
{
    public class Location
    {
        [JsonProperty("success")]
        public bool Success { get; internal set; } = true;
        [JsonProperty("continent_name")]
        public string Continent { get; internal set; }
        [JsonProperty("country_name")]
        public string Country { get; internal set; }
    }
}
