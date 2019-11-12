using Newtonsoft.Json;
using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace OurUmbraco.Auth
{
    [TableName("identityAuthTokens")]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    public class UmbracoAuthToken
    {
        [Column("pk")]
        [PrimaryKeyColumn]
        [JsonIgnore]
        public int PrimaryKey { get; set; }

        [Column("memberId")]
        [JsonProperty("member_id")]
        public int MemberId { get; set; }

        [Column("projectId")]
        [JsonProperty("project_id")]
        public int ProjectId { get; set; }

        [Column("dateCreated")]
        [JsonProperty("date_created")]
        public DateTime DateCreated { get; set; }

        [Column("authToken")]
        [JsonIgnore]
        public string AuthToken { get; set; }
    }
}
