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
        public int PrimaryKey { get; set; }

        [Column("memberId")]
        public int MemberId { get; set; }

        [Column("projectId")]
        public int ProjectId { get; set; }

        [Column("dateCreated")]
        public DateTime DateCreated { get; set; }

        [Column("authToken")]
        public string AuthToken { get; set; }
    }
}
