using Newtonsoft.Json;
using System.Collections.Generic;

namespace OurUmbraco.Forum.Models
{
    internal class ErrorModel
    {
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("errors")]
        public List<string> Errors { get; set; }
    }
}
