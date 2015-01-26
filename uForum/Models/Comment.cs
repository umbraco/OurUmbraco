using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;

namespace uForum.Models
{
    [TableName("forumComments")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    public class Comment
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("topicId")]
        public int TopicId { get; set; }

        [Column("parentCommentId")]
        public int ParentCommentId { get; set; }

        [Column("memberId")]
        public int MemberId { get; set; }
        
        [Column("position")]
        public int Position { get; set; }

        [Column("body")]
        public string Body { get; set; }
        
        [Column("created")]
        public DateTime Created { get; set; }

        [Column("score")]
        public int Score { get; set; }

        [Column("isSpam")]
        public bool IsSpam { get; set; }

        [Column("hasChildren")]
        public bool HasChildren { get; set; }
    }
}
