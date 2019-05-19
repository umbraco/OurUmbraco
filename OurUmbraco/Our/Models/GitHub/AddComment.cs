using Newtonsoft.Json;

namespace OurUmbraco.Our.Models.GitHub
{
   public class AddComment
    {
        [JsonProperty("body")]
        public string CommentBody { get; set; }
    }
}
