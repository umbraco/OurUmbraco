using Newtonsoft.Json;
using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace OurUmbraco.Auth
{
    /// <summary>
    /// An authentication key used to authenticate members/projects
    /// </summary>
    [TableName("projectAuthKeys")]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    public class ProjectAuthKey
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
        
        [Column("description")]
        [JsonProperty("description")]
        public string Description { get; set; }

        [Column("authKey")]
        [JsonProperty("authKey")]
        public string AuthKey { get; set; }
    }
}
