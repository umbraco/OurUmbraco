using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uForum.Models
{
    [PetaPoco.TableName("forumTopics")]
    [PetaPoco.PrimaryKey("id")]
    [PetaPoco.ExplicitColumns]
    public class Topic
    {
        [PetaPoco.Column("id")]
        public int Id { get; set; }

        [PetaPoco.Column("parentId")]
        public int ParentId { get; set; }
        
        [PetaPoco.Column("memberId")]
        public int MemberId { get; set; }
        
        [PetaPoco.Column("answer")]
        public int Answer { get; set; }

        [PetaPoco.Column("title")]
        public string Title { get; set; }
        
        [PetaPoco.Column("urlName")]
        public string UrlName { get; set; }
        
        [PetaPoco.Column("body")]
        public string Body { get; set; }
        
        [PetaPoco.Column("created")]
        public DateTime Created { get; set; }
        
        [PetaPoco.Column("updated")]
        public DateTime Updated { get; set; }

        [PetaPoco.Column("isSpam")]
        public bool IsSpam { get; set; }

        [PetaPoco.Column("latestReplyAuthor")]
        public int LatestReplyAuthor { get; set; }

        [PetaPoco.Column("latestComment")]
        public int LatestComment { get; set; }

        [PetaPoco.Column("replies")]
        public int Replies { get; set; }

        [PetaPoco.Column("score")]
        public int Score { get; set; }

        [PetaPoco.Column("locked")]
        public bool Locked { get; set; }
    }
}
