using System.Collections.Generic;
using Newtonsoft.Json;

namespace OurUmbraco.NuGet.Models
{
    public class ApiResponse
    {
        [JsonProperty("@context")]
        public Context Context { get; set; }

        [JsonProperty("totalHits")]
        public int TotalHits { get; set; }

        [JsonProperty("data")]
        public List<SearchResult> SearchResults { get; set; }
    }
}