using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;

namespace uForum.Models
{
    [TableName("forumComments")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    [DataContract(Name = "comment", Namespace = "")]
    public class Comment
    {
        [Column("id")]
        [DataMember(Name = "id")]
        public int Id { get; set; }
        
        [Column("topicId")]
        [DataMember(Name = "topicId")]
        public int TopicId { get; set; }

        [Column("parentCommentId")]
        [DataMember(Name = "parentCommentId")]
        public int ParentCommentId { get; set; }

        [Column("memberId")]
        [DataMember(Name = "memberId")]
        public int MemberId { get; set; }
        
        [Column("position")]
        [DataMember(Name = "position")]
        public int Position { get; set; }

        [Column("body")]
        [DataMember(Name = "body")]
        public string Body { get; set; }
        
        [Column("created")]
        [DataMember(Name = "created")]
        public DateTime Created { get; set; }

        [Column("score")]
        [DataMember(Name = "score")]
        public int Score { get; set; }

        [Column("isSpam")]
        [DataMember(Name = "isSpam")]
        public bool IsSpam { get; set; }

        [Column("hasChildren")]
        [DataMember(Name = "hasChildren")]
        public bool HasChildren { get; set; }
    }
}
