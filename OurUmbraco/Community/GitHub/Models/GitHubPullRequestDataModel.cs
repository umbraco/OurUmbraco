using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;

namespace OurUmbraco.Community.GitHub.Models
{
    [TableName("GitHubPullRequest")]
    [PrimaryKey("id", autoIncrement = false)]
    public class GitHubPullRequestDataModel
    {
        [Column("id")]
        public string Id { get; set; }
        [Column("number")]
        public int Number { get; set; }
        [Column("url")]
        public string Url { get; set; }
        [Column("authorUrl")]
        public string AuthorUrl { get; set; }
        [Column("authorLogin")]
        public string AuthorLogin { get; set; }
        [Column("authorAvatarUrl")]
        public string AuthorAvatarUrl { get; set; }
        [Column("additions")]
        public int? Additions { get; set; }
        [Column("deletions")]
        public int? Deletions { get; set; }
        [Column("created")]
        public DateTime Created { get; set; }
        [Column("lastModified")]
        public DateTime LastModified { get; set; }
    }
}
