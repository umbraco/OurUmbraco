using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uForum.Models
{
    [PetaPoco.TableName("forumComments")]
    [PetaPoco.PrimaryKey("id")]
    [PetaPoco.ExplicitColumns]
    public class Comment
    {
        [PetaPoco.Column("id")]
        public int Id { get; set; }
        
        [PetaPoco.Column("topicId")]
        public int TopicId { get; set; }

        [PetaPoco.Column("parentCommentId")]
        public int ParentCommentId { get; set; }

        [PetaPoco.Column("memberId")]
        public int MemberId { get; set; }
        
        [PetaPoco.Column("position")]
        public int Position { get; set; }

        [PetaPoco.Column("body")]
        public string Body { get; set; }
        
        [PetaPoco.Column("created")]
        public DateTime Created { get; set; }

        [PetaPoco.Column("score")]
        public int Score { get; set; }

        [PetaPoco.Column("isSpam")]
        public bool IsSpam { get; set; }

        [PetaPoco.Column("hasChildren")]
        public bool HasChildren { get; set; }
    }
}
