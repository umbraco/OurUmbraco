using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace OurUmbraco.Our.Models.GitHub.AutoReplies
{

    [TableName(TableName)]
    [PrimaryKey(PrimaryKey, autoIncrement = true)]
    public class GitHubAutoReplyPoco
    {

        public const string TableName = "GitHubAutoReplies";

        public const string PrimaryKey = "Id";

        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        public string Repository { get; set; }

        public int Number { get; set; }

        public int Type { get; set; }

        public long CommentId { get; set; }

        public DateTime CreateDate { get; set; }

    }

}