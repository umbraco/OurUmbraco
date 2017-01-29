using System;
using System.Collections.Generic;
using OurUmbraco.Our.Models;
using Umbraco.Core.Persistence;

namespace OurUmbraco.Forum.Models
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

        [Column("topicAuthorId")]
        public int TopicAuthorId { get; set; }

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

        public MemberData MemberData { get; set; }

        public bool IsAnswer { get; set; }

        public IEnumerable<TopicMember> TopicMembers { get; set; }

        public bool ForumNewTopicsAllowed { get; set; }

        public List<SimpleMember> Votes { get; set; }
    }
}