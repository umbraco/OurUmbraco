using System;
using System.Runtime.Serialization;
using Umbraco.Core.Persistence;

namespace uForum.Models
{
    [TableName("forumComments")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    public class ReadOnlyComment
    {
        [Column("commentId")]
        public int Id { get; set; }

        [Column("topicId")]
        public int TopicId { get; set; }

        [Column("parentCommentId")]
        public int ParentCommentId { get; set; }

        [Column("commentMemberId")]
        public int MemberId { get; set; }

        [Column("position")]
        public int Position { get; set; }

        [Column("commentBody")]
        public string Body { get; set; }

        [Column("commentCreated")]
        public DateTime Created { get; set; }

        [Column("score")]
        public int Score { get; set; }

        [Column("commentIsSpam")]
        public bool IsSpam { get; set; }

        [Column("hasChildren")]
        public bool HasChildren { get; set; }


    }
}