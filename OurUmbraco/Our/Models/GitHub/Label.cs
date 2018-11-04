using Newtonsoft.Json;

namespace OurUmbraco.Our.Models.GitHub
{
    public class Label
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        // Note: leaving the other properties commented out in case we need them later

        //public int id { get; set; }
        //public string node_id { get; set; }
        //public string url { get; set; }
        //public string color { get; set; }
        //public bool _default { get; set; }
    }
}
